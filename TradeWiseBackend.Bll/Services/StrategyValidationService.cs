using System;
using TradeWiseBackend.Domain.Models;

namespace TradeWiseBackend.Bll.Services;

public class StrategyValidationService(List<StrategyStage> stages, List<StrategyTransition> transitions)
{
    public (bool IsValid, string? ErrorMessage) Validate()
    {
        var stageIds = GetStageIds();

        var check = CheckEmptyness();
        if (!check.IsValid) return check;

        check = CheckUnknownStagesInTransitions();
        if (!check.IsValid) return check;

        check = CheckInvalidSelfTransitions();
        if (!check.IsValid) return check;

        check = CheckNullSourceDestinationTransitions();
        if (!check.IsValid) return check;

        check = CheckSingleNullStartEndTransitions();
        if (!check.IsValid) return check;

        var adjacency = BuildAdjacency(stageIds);

        var possibleRoots = FindRoots(stageIds);
        if (possibleRoots.Count != 1)
            return (false, "There must be exactly one root of the graph (a vertex with no incoming edges)");

        var root = possibleRoots[0];
        var visited = new HashSet<Guid>();

        if (!Dfs(root, adjacency, ref visited))
            return (false, "A cycle has been detected in the graph");

        if (visited.Count != stageIds.Count)
            return (false, "The graph is disconnected, there are unreachable nodes");

        return (true, null);
    }

    private HashSet<Guid> GetStageIds()
    {
        return new HashSet<Guid>(stages.Select(s => s.Id));
    }

    private (bool IsValid, string? ErrorMessage) CheckEmptyness() {
        if(transitions.Count == 0 || stages.Count == 0) {
            return (false, "List of transitions or stages is empty");
        }
        return (true, null);
    }

    private (bool IsValid, string? ErrorMessage) CheckUnknownStagesInTransitions()
    {
        var stageIdMap = GetStageIds();
        foreach (var transition in transitions) {
            if (transition.SourceStageId.HasValue && !stageIdMap.Contains(transition.SourceStageId.Value)) {
                return (false, "The transition contains an unknown start stage " + transition.SourceStageId.Value);
            }

            if (transition.DestinationStageId.HasValue && !stageIdMap.Contains(transition.DestinationStageId.Value)) {
                return (false, "The transition contains an unknown end stage " + transition.DestinationStageId.Value);
            }
        }
        return (true, null);
    }

    private (bool IsValid, string? ErrorMessage) CheckInvalidSelfTransitions()
    {
        if (transitions.Any(t => t.SourceStageId == t.DestinationStageId && t.SourceStageId != null))
            return (false, "The start and end of the transition cannot match");
        return (true, null);
    }

    private (bool IsValid, string? ErrorMessage) CheckNullSourceDestinationTransitions()
    {
        if (transitions.Any(t => t.SourceStageId == null && t.DestinationStageId == null))
            return (false, "Start and end of the transition are empty");
        return (true, null);
    }

    private (bool IsValid, string? ErrorMessage) CheckSingleNullStartEndTransitions()
    {
        if (transitions.Count(t => t.SourceStageId == null) != 1)
            return (false, "Only one node with an empty start is expected");
        return (true, null);
    }

    private Dictionary<Guid, List<Guid>> BuildAdjacency(HashSet<Guid> stageIds)
    {
        var adjacency = new Dictionary<Guid, List<Guid>>();
        foreach (var stageId in stageIds)
            adjacency[stageId] = new List<Guid>();

        foreach (var t in transitions)
        {
            if (t.SourceStageId != null && t.DestinationStageId != null)
            {
                adjacency[t.SourceStageId.Value].Add(t.DestinationStageId.Value);
            }
        }

        return adjacency;
    }

    private List<Guid> FindRoots(HashSet<Guid> stageIds)
    {
        var allDestinations = new HashSet<Guid>(transitions.Where(t => t.DestinationStageId != null).Select(t => t.DestinationStageId!.Value));
        return stageIds.Except(allDestinations).ToList();
    }

    private static bool Dfs(Guid node, Dictionary<Guid, List<Guid>> adjacency, ref HashSet<Guid> visited)
    {
        Console.WriteLine("start visited: " + string.Join(", ", visited));

        if (visited.Contains(node))
            return false;

        visited.Add(node);

        Console.WriteLine("end visited: " + string.Join(", ", visited));
        foreach (var linkedNode in adjacency[node])
        {
            if (!Dfs(linkedNode, adjacency, ref visited))
                return false;
        }

        return true;
    }
}
