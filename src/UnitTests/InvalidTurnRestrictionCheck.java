package org.openstreetmap.atlas.checks.validation.relations;

import java.util.Collections;
import java.util.HashMap;
import java.util.List;
import java.util.Map;
import java.util.Optional;
import java.util.Set;
import java.util.stream.Collectors;

import org.openstreetmap.atlas.checks.base.BaseCheck;
import org.openstreetmap.atlas.checks.flag.CheckFlag;
import org.openstreetmap.atlas.geography.atlas.items.AtlasObject;
import org.openstreetmap.atlas.geography.atlas.items.Relation;
import org.openstreetmap.atlas.geography.atlas.items.RelationMember;
import org.openstreetmap.atlas.geography.atlas.items.TurnRestriction;
import org.openstreetmap.atlas.tags.RelationTypeTag;
import org.openstreetmap.atlas.tags.TurnRestrictionTag;
import org.openstreetmap.atlas.utilities.configuration.Configuration;

/**
 * Check for invalid turn restrictions
 *
 * @author gpogulsky
 * @author bbreithaupt
 */
public class InvalidTurnRestrictionCheck extends BaseCheck<Long>
{
    private static final List<String> FALLBACK_INSTRUCTIONS = Collections.singletonList(
            "Relation ID: {0,number,#} is marked as turn restriction, but it is not well-formed: {1}");
    private static final String MISSING_TO_FROM_INSTRUCTION = "Missing a from and/or to member";
    private static final String UNKNOWN_ISSUE = "Unable to specify issue";
    private static final Map<String, String> INVALID_REASON_INSTRUCTION_MAP = new HashMap<>();
    private static final long serialVersionUID = -983698716949386657L;

    static
    {
        final String routeInstruction = "There is not a single navigable route to restrict, this restriction may be redundant or need to be split in to multiple relations";
        INVALID_REASON_INSTRUCTION_MAP.put("Cannot have a route with no members", routeInstruction);
        INVALID_REASON_INSTRUCTION_MAP.put(
                "Restriction relation should not have more than 1 via node.",
                "A Turn Restriction should only have 1 via Node");
        INVALID_REASON_INSTRUCTION_MAP.put(
                "has same members in from and to, but has no via members to disambiguate.",
                "Via member is required for restrictions with the same to and from members");
        INVALID_REASON_INSTRUCTION_MAP.put("Can't build route from", routeInstruction);
        INVALID_REASON_INSTRUCTION_MAP.put("Unable to build a route from edges", routeInstruction);
        INVALID_REASON_INSTRUCTION_MAP.put(
                "A route was found from start to end, but not every unique edge was used",
                routeInstruction);
        INVALID_REASON_INSTRUCTION_MAP.put("No edge that connects to the current route",
                routeInstruction);
    }

    /**
     * Default constructor
     *
     * @param configuration
     *            the JSON configuration for this check
     */
    public InvalidTurnRestrictionCheck(final Configuration configuration)
    {
        super(configuration);
    }

    @Override
    public boolean validCheckForObject(final AtlasObject object)
    {
        return object instanceof Relation && TurnRestrictionTag.isRestriction(object);
    }

    @Override
    protected Optional<CheckFlag> flag(final AtlasObject object)
    {
        final Relation relation = (Relation) object;
        final Set<AtlasObject> members = relation.members().stream().map(RelationMember::getEntity)
                .collect(Collectors.toSet());

        // A to and from member are required
        if (relation.members().stream()
                .noneMatch(member -> member.getRole().equals(RelationTypeTag.RESTRICTION_ROLE_FROM))
                || relation.members().stream().noneMatch(
                        member -> member.getRole().equals(RelationTypeTag.RESTRICTION_ROLE_TO)))
        {
            return Optional.of(createFlag(members, this.getLocalizedInstruction(0,
                    relation.getOsmIdentifier(), MISSING_TO_FROM_INSTRUCTION)));
        }

        // Build a turn restriction
        final TurnRestriction turnRestriction = new TurnRestriction(relation);
        // If it is not valid map the reason to an instruction
        if (!turnRestriction.isValid())
        {
            return Optional.of(createFlag(members, this.getLocalizedInstruction(0,
                    relation.getOsmIdentifier(),
                    this.getInstructionFromInvalidReason(turnRestriction.getInvalidReason()))));
        }

        return Optional.empty();

    }

    @Override
    protected List<String> getFallbackInstructions()
    {
        return FALLBACK_INSTRUCTIONS;
    }

    /**
     * Map {@link TurnRestriction} invalid reasons to instructions
     *
     * @param invalidReason
     *            invalid reason from {@link TurnRestriction}
     * @return {@link String} instruction
     */
    private String getInstructionFromInvalidReason(final String invalidReason)
    {
        String instruction = UNKNOWN_ISSUE;

        for (final Map.Entry<String, String> entry : INVALID_REASON_INSTRUCTION_MAP.entrySet())
        {
            if (invalidReason.contains(entry.getKey()))
            {
                instruction = entry.getValue();
                break;
            }
        }

        return instruction;
    }

}
