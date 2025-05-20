using System;
using System.ComponentModel.DataAnnotations;
using TradeWiseBackend.Domain.Models;

namespace TradeWiseBackend.Bll.Services;

public class StrategyValidationService()
{
    public (bool IsValid, string? ErrorMessage) Validate(List<StrategyStage> stages, List<StrategyTransition> transitions)
    {
        var stageIds = GetStageIds(stages);

        var check = CheckEmptyness(stages, transitions);
        if (!check.IsValid) return check;

        check = CheckUnknownStagesInTransitions(stages, transitions);
        if (!check.IsValid) return check;

        check = CheckInvalidSelfTransitions(transitions);
        if (!check.IsValid) return check;

        check = CheckNullSourceDestinationTransitions(transitions);
        if (!check.IsValid) return check;

        check = CheckCountNullStartEndTransitions(transitions);
        if (!check.IsValid) return check;

        var adjacency = BuildAdjacency(stageIds, transitions);

        var root = FindRoot(stages, transitions);

        var visited = new HashSet<Guid>();

        if (!Dfs(root, adjacency, ref visited))
            return (false, "A cycle has been detected in the graph");

        if (visited.Count != stageIds.Count)
            return (false, "The graph is disconnected, there are unreachable nodes");

        return (true, null);
    }

    public (bool IsValid, string? ErrorMessage) PreValidate(List<StrategyStage> stages, List<StrategyTransition> transitions)
    {
        var stageIds = GetStageIds(stages);

        var check = CheckUnknownStagesInTransitions(stages, transitions);
        if (!check.IsValid) return check;

        check = CheckInvalidSelfTransitions(transitions);
        if (!check.IsValid) return check;

        check = CheckNullSourceDestinationTransitions(transitions);
        if (!check.IsValid) return check;

        check = CheckCountNullStartEndTransitions(transitions);
        if (!check.IsValid) return check;

        var adjacency = BuildAdjacency(stageIds, transitions);

        var root = FindRoot(stages, transitions);

        var visited = new HashSet<Guid>();

        if (!Dfs(root, adjacency, ref visited))
            return (false, "A cycle has been detected in the graph");

        return (true, null);
    }

    private HashSet<Guid> GetStageIds(List<StrategyStage> stages)
    {
        return new HashSet<Guid>(stages.Select(s => s.Id));
    }

    private (bool IsValid, string? ErrorMessage) CheckEmptyness(List<StrategyStage> stages, List<StrategyTransition> transitions)
    {
        if (transitions.Count == 0 || stages.Count == 0)
        {
            return (false, "List of transitions or stages is empty");
        }
        return (true, null);
    }

    private (bool IsValid, string? ErrorMessage) CheckUnknownStagesInTransitions(List<StrategyStage> stages, List<StrategyTransition> transitions)
    {
        var stageIdMap = GetStageIds(stages);
        foreach (var transition in transitions)
        {
            if (transition.SourceStageId.HasValue && !stageIdMap.Contains(transition.SourceStageId.Value))
            {
                return (false, "The transition contains an unknown start stage " + transition.SourceStageId.Value);
            }

            if (transition.DestinationStageId.HasValue && !stageIdMap.Contains(transition.DestinationStageId.Value))
            {
                return (false, "The transition contains an unknown end stage " + transition.DestinationStageId.Value);
            }
        }
        return (true, null);
    }

    private (bool IsValid, string? ErrorMessage) CheckInvalidSelfTransitions(List<StrategyTransition> transitions)
    {
        if (transitions.Any(t => t.SourceStageId == t.DestinationStageId && t.SourceStageId != null))
            return (false, "The start and end of the transition cannot match");
        return (true, null);
    }

    private (bool IsValid, string? ErrorMessage) CheckNullSourceDestinationTransitions(List<StrategyTransition> transitions)
    {
        if (transitions.Any(t => t.SourceStageId == null && t.DestinationStageId == null))
            return (false, "Start and end of the transition are empty");
        return (true, null);
    }

    private (bool IsValid, string? ErrorMessage) CheckCountNullStartEndTransitions(List<StrategyTransition> transitions)
    {
        if (transitions.Count(t => t.SourceStageId == null) != 1)
            return (false, "Only one transition with an empty start is expected");

        if (!transitions.Any(t => t.DestinationStageId == null))
            return (false, "There is no any transitions with empty end");

        return (true, null);
    }

    private Dictionary<Guid, List<Guid>> BuildAdjacency(HashSet<Guid> stageIds, List<StrategyTransition> transitions)
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

    private Guid FindRoot(List<StrategyStage> stages, List<StrategyTransition> transitions)
    {
        var rootId = transitions.SingleOrDefault(t => t.SourceStageId == null)?.DestinationStageId ??
               throw new ValidationException("No root found for strategy");

        if (stages.All(stage => stage.Id != rootId))
            throw new ValidationException("Found transition to root node, but the node itself does not exist!");

        return rootId;
    }
    private static bool Dfs(Guid node, Dictionary<Guid, List<Guid>> adjacency, ref HashSet<Guid> visited)
    {
        if (visited.Contains(node))
            return false;

        visited.Add(node);

        foreach (var linkedNode in adjacency[node])
        {
            if (!Dfs(linkedNode, adjacency, ref visited))
                return false;
        }

        return true;
    }
}
