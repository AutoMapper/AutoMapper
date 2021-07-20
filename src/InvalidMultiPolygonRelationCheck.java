package org.openstreetmap.atlas.checks.validation.relations;

import java.util.ArrayList;
import java.util.Arrays;
import java.util.Collections;
import java.util.EnumMap;
import java.util.HashSet;
import java.util.LinkedHashSet;
import java.util.List;
import java.util.Optional;
import java.util.Set;
import java.util.stream.Collectors;
import java.util.stream.Stream;

import org.openstreetmap.atlas.checks.base.BaseCheck;
import org.openstreetmap.atlas.checks.flag.CheckFlag;
import org.openstreetmap.atlas.checks.utility.CommonMethods;
import org.openstreetmap.atlas.exception.CoreException;
import org.openstreetmap.atlas.geography.Location;
import org.openstreetmap.atlas.geography.MultiPolygon;
import org.openstreetmap.atlas.geography.PolyLine;
import org.openstreetmap.atlas.geography.Polygon;
import org.openstreetmap.atlas.geography.atlas.items.AtlasObject;
import org.openstreetmap.atlas.geography.atlas.items.ItemType;
import org.openstreetmap.atlas.geography.atlas.items.Line;
import org.openstreetmap.atlas.geography.atlas.items.Relation;
import org.openstreetmap.atlas.geography.atlas.items.RelationMember;
import org.openstreetmap.atlas.geography.atlas.items.complex.RelationOrAreaToMultiPolygonConverter;
import org.openstreetmap.atlas.geography.converters.MultiplePolyLineToPolygonsConverter;
import org.openstreetmap.atlas.geography.converters.jts.JtsPolygonConverter;
import org.openstreetmap.atlas.tags.RelationTypeTag;
import org.openstreetmap.atlas.tags.SyntheticInvalidGeometryTag;
import org.openstreetmap.atlas.tags.SyntheticRelationMemberAdded;
import org.openstreetmap.atlas.tags.annotations.validation.Validators;
import org.openstreetmap.atlas.utilities.configuration.Configuration;
import org.openstreetmap.atlas.utilities.maps.MultiMap;
import org.openstreetmap.atlas.utilities.scalars.Angle;
import org.openstreetmap.atlas.utilities.tuples.Tuple;
import org.slf4j.Logger;
import org.slf4j.LoggerFactory;

/**
 * Check designed to scan through MultiPolygon relations and flag them for any and all reasons they
 * are invalid:
 * <ul>
 * <li>The multipolygon must be closed.</li>
 * <li>There must one or more outer members</li>
 * <li>Each member must have a role</li>
 * <li>There should be more than one member (Optional)</li>
 * <li>Inner members must be contained by an outer member, but not intersect any</li>
 * <li>Outer members must not overlap</li>
 * <li>Inner members must not overlap with, but may touch, other inner members</li>
 * </ul>
 *
 * @author jklamer
 * @author sid
 * @author bbreithaupt
 */
public class InvalidMultiPolygonRelationCheck extends BaseCheck<Long>
{

    public static final int CLOSED_LOOP_INSTRUCTION_FORMAT_INDEX;
    public static final int INVALID_OSM_TYPE_INSTRUCTION_FORMAT_INDEX;
    public static final int INVALID_ROLE_INSTRUCTION_FORMAT_INDEX;
    public static final int MISSING_OUTER_INSTRUCTION_FORMAT_INDEX;
    public static final int SINGLE_MEMBER_RELATION_INSTRUCTION_FORMAT_INDEX;
    public static final int INVALID_OVERLAP_INSTRUCTION_FORMAT_INDEX;
    public static final int INNER_MISSING_OUTER_INSTRUCTION_FORMAT_INDEX;
    public static final int GENERIC_INVALID_GEOMETRY_INSTRUCTION_FORMAT_INDEX;
    private static final String CLOSED_LOOP_INSTRUCTION_FORMAT = "The Multipolygon relation {0,number,#} with members : {1} is not closed at some locations : {2}";
    private static final String INVALID_OSM_TYPE_INSTRUCTION_FORMAT = "{0} relation member(s) are an invalid type in relation {1,number,#}. Multipolygon relations can only have ways as members. The first object id(s) are {2}";
    private static final String INVALID_ROLE_INSTRUCTION_FORMAT = "{0} ways have an invalid or missing role in multipolygon relation {1,number,#}. The role must be either outer or inner. The way id(s) are {2}";
    private static final String INVALID_OVERLAP_INSTRUCTION_FORMAT = "Relation {0,number,#} has members with centroids {1} & {2} that invalidly overlap.";
    private static final String INNER_MISSING_OUTER_INSTRUCTION_FORMAT = "Relation {0,number,#} has an inner member that is not contained by an outer member.";
    private static final String MISSING_OUTER_INSTRUCTION_FORMAT = "Multipolygon relation {0,number,#} has no outer member(s). Must have 1 or more.";
    private static final String SINGLE_MEMBER_RELATION_INSTRUCTION_FORMAT = "Multipolygon relation {0,number,#} has only one member.";
    private static final String GENERIC_INVALID_GEOMETRY_INSTRUCTION_FORMAT = "Multipolygon relation {0,number,#} has invalid geometry.";
    private static final List<String> FALLBACK_INSTRUCTIONS = Arrays.asList(
            CLOSED_LOOP_INSTRUCTION_FORMAT, SINGLE_MEMBER_RELATION_INSTRUCTION_FORMAT,
            MISSING_OUTER_INSTRUCTION_FORMAT, INVALID_ROLE_INSTRUCTION_FORMAT,
            INVALID_OSM_TYPE_INSTRUCTION_FORMAT, INVALID_OVERLAP_INSTRUCTION_FORMAT,
            INNER_MISSING_OUTER_INSTRUCTION_FORMAT, GENERIC_INVALID_GEOMETRY_INSTRUCTION_FORMAT);
    private static final RelationOrAreaToMultiPolygonConverter RELATION_OR_AREA_TO_MULTI_POLYGON_CONVERTER = new RelationOrAreaToMultiPolygonConverter();
    private static final EnumMap<ItemType, String> atlasToOsmType = new EnumMap<>(ItemType.class);
    private static final Logger logger = LoggerFactory
            .getLogger(InvalidMultiPolygonRelationCheck.class);

    private static final JtsPolygonConverter JTS_POLYGON_CONVERTER = new JtsPolygonConverter();
    private static final long OVERLAP_MINIMUM_POINTS_DEFAULT = 0;
    private static final long OVERLAP_MAMIMUM_POINTS_DEFAULT = 300000;

    static
    {
        INVALID_ROLE_INSTRUCTION_FORMAT_INDEX = FALLBACK_INSTRUCTIONS
                .indexOf(INVALID_ROLE_INSTRUCTION_FORMAT);
        MISSING_OUTER_INSTRUCTION_FORMAT_INDEX = FALLBACK_INSTRUCTIONS
                .indexOf(MISSING_OUTER_INSTRUCTION_FORMAT);
        SINGLE_MEMBER_RELATION_INSTRUCTION_FORMAT_INDEX = FALLBACK_INSTRUCTIONS
                .indexOf(SINGLE_MEMBER_RELATION_INSTRUCTION_FORMAT);
        CLOSED_LOOP_INSTRUCTION_FORMAT_INDEX = FALLBACK_INSTRUCTIONS
                .indexOf(CLOSED_LOOP_INSTRUCTION_FORMAT);
        INVALID_OSM_TYPE_INSTRUCTION_FORMAT_INDEX = FALLBACK_INSTRUCTIONS
                .indexOf(INVALID_OSM_TYPE_INSTRUCTION_FORMAT);
        INVALID_OVERLAP_INSTRUCTION_FORMAT_INDEX = FALLBACK_INSTRUCTIONS
                .indexOf(INVALID_OVERLAP_INSTRUCTION_FORMAT);
        INNER_MISSING_OUTER_INSTRUCTION_FORMAT_INDEX = FALLBACK_INSTRUCTIONS
                .indexOf(INNER_MISSING_OUTER_INSTRUCTION_FORMAT);
        GENERIC_INVALID_GEOMETRY_INSTRUCTION_FORMAT_INDEX = FALLBACK_INSTRUCTIONS
                .indexOf(GENERIC_INVALID_GEOMETRY_INSTRUCTION_FORMAT);
        atlasToOsmType.put(ItemType.EDGE, "way");
        atlasToOsmType.put(ItemType.AREA, "way");
        atlasToOsmType.put(ItemType.LINE, "way");
        atlasToOsmType.put(ItemType.NODE, "node");
        atlasToOsmType.put(ItemType.POINT, "node");
        atlasToOsmType.put(ItemType.RELATION, "relation");
    }

    private final boolean ignoreOneMember;
    private final long overlapMinimumPoints;
    private final long overlapMaximumPoints;

    public InvalidMultiPolygonRelationCheck(final Configuration configuration)
    {
        super(configuration);
        this.ignoreOneMember = this.configurationValue(configuration, "members.one.ignore", false);
        this.overlapMinimumPoints = this.configurationValue(configuration, "overlap.points.minimum",
                OVERLAP_MINIMUM_POINTS_DEFAULT);
        this.overlapMaximumPoints = this.configurationValue(configuration, "overlap.points.maximum",
                OVERLAP_MAMIMUM_POINTS_DEFAULT);
    }

    @Override
    public boolean validCheckForObject(final AtlasObject object)
    {
        return object instanceof Relation
                && Validators.isOfType(object, RelationTypeTag.class, RelationTypeTag.MULTIPOLYGON)
                && !(this.ignoreOneMember
                        && CommonMethods.getOSMRelationMemberSize((Relation) object) == 1)
                && !SyntheticRelationMemberAdded.hasAddedRelationMember(object);
    }

    @Override
    protected Optional<CheckFlag> flag(final AtlasObject object)
    {
        final Relation multipolygonRelation = (Relation) object;
        final List<String> instructions = new ArrayList<>();
        final Set<Location> issueLocations = new HashSet<>();

        if (Validators.hasValuesFor(object, SyntheticInvalidGeometryTag.class))
        {
            instructions.add(
                    this.getLocalizedInstruction(GENERIC_INVALID_GEOMETRY_INSTRUCTION_FORMAT_INDEX,
                            multipolygonRelation.getOsmIdentifier()));
        }

        this.checkGeometry(multipolygonRelation).ifPresent(tuple ->
        {
            instructions.addAll(tuple.getFirst());
            issueLocations.addAll(tuple.getSecond());
        });

        if (multipolygonRelation.members().size() <= 1)
        {
            instructions.add(
                    this.getLocalizedInstruction(SINGLE_MEMBER_RELATION_INSTRUCTION_FORMAT_INDEX,
                            multipolygonRelation.getOsmIdentifier()));
        }

        instructions.addAll(this.checkRolesAndTypes(multipolygonRelation));

        if (!instructions.isEmpty())
        {
            final CheckFlag flag = this.createFlag(object, instructions);
            flag.addPoints(issueLocations);
            return Optional.of(flag);
        }

        return Optional.empty();
    }

    @Override
    protected List<String> getFallbackInstructions()
    {
        return FALLBACK_INSTRUCTIONS;
    }

    /**
     * Check that a multipolygon {@link Relation} has valid geometry.
     *
     * @param multipolygonRelation
     *            {@link Relation} of type multipolygon
     * @return an Optional containing a {@link Tuple} containing a {@link Set} of {@link String}
     *         instructions for invalid geometries and a {@link Set} of {@link Location}s marking
     *         the invalid geometries
     */
    private Optional<Tuple<Set<String>, Set<Location>>> checkGeometry(
            final Relation multipolygonRelation)
    {
        // Try converting the Relation to a MultiPolygon. If it works check that the geometries
        // don't overlap.
        try
        {
            final MultiPolygon multiPolygon = RELATION_OR_AREA_TO_MULTI_POLYGON_CONVERTER
                    .convert(multipolygonRelation);
            // Skip the overlap checks for multipolygons outside the configurable range of shape
            // points
            final long shapePoints = multiPolygon.getOuterToInners().entrySet().stream()
                    .mapToInt(entry -> entry.getKey().size()
                            + entry.getValue().stream().mapToInt(PolyLine::size).sum())
                    .sum();
            if (this.overlapMinimumPoints <= shapePoints
                    && shapePoints <= this.overlapMaximumPoints)
            {
                return Optional.of(
                        this.checkOverlap(multiPolygon, multipolygonRelation.getOsmIdentifier()));
            }
            return Optional.empty();
        }
        // Catch open polygons and mark the broken locations
        catch (final MultiplePolyLineToPolygonsConverter.OpenPolygonException exception)
        {
            final List<Location> openLocations = exception.getOpenLocations();
            final Set<AtlasObject> objects = openLocations.stream()
                    .flatMap(location -> this.filterMembers(multipolygonRelation, location))
                    .collect(Collectors.toSet());
            final Set<Long> memberIds = multipolygonRelation.members().stream()
                    .map(member -> member.getEntity().getOsmIdentifier())
                    .collect(Collectors.toSet());

            if (!objects.isEmpty() && !memberIds.isEmpty())
            {
                return Optional.of(Tuple.createTuple(
                        Collections.singleton(this.getLocalizedInstruction(
                                CLOSED_LOOP_INSTRUCTION_FORMAT_INDEX,
                                multipolygonRelation.getOsmIdentifier(), memberIds, openLocations)),
                        new HashSet<>(openLocations)));
            }
            else
            {
                logger.warn(
                        "Unable to find members in multipolygonRelation {} containing the locations : {}",
                        multipolygonRelation, openLocations);
            }
        }
        catch (final CoreException exception)
        {
            // Catch multipolygon relations with no outer members
            if (exception.getMessage().equals("Unable to find outer polygon."))
            {
                return Optional.of(Tuple.createTuple(
                        Collections.singleton(
                                this.getLocalizedInstruction(MISSING_OUTER_INSTRUCTION_FORMAT_INDEX,
                                        multipolygonRelation.getOsmIdentifier())),
                        Collections.emptySet()));
            }
            // Catch inner members that are not inside an outer member
            if (exception.getMessage().contains("Malformed MultiPolygon: inner has no outer host"))
            {
                return Optional
                        .of(Tuple.createTuple(
                                Collections.singleton(this.getLocalizedInstruction(
                                        INNER_MISSING_OUTER_INSTRUCTION_FORMAT_INDEX,
                                        multipolygonRelation.getOsmIdentifier())),
                                Collections.emptySet()));
            }

            // Ignore other core exceptions
            logger.warn("Unable to convert multipolygonRelation {}. {}",
                    multipolygonRelation.getOsmIdentifier(), exception.getMessage());
        }
        catch (final Exception exception)
        {
            logger.warn("Unable to convert multipolygonRelation {}. {}",
                    multipolygonRelation.getOsmIdentifier(), exception.getMessage());
        }

        return Optional.empty();
    }

    /**
     * Check that inner polygons do not intersect any outers and do not overlap other inners. Inners
     * are allowed to touch other inners.
     *
     * @param outerToInners
     *            {@link MultiMap} of outer {@link Polygon}s to inner {@link Polygon}s
     * @return a {@link Set} of {@link Tuple}s containing {@link Polygon}s that invalidly overlap
     */
    private Set<Tuple<Polygon, Polygon>> checkInnerOverlap(
            final MultiMap<Polygon, Polygon> outerToInners)
    {
        final Set<Tuple<Polygon, Polygon>> problematicPolygons = new HashSet<>();

        outerToInners.forEach((key, value) ->
        {
            // Loop through each combination of inner polygons only once to check for overlap
            for (int index1 = 0; index1 < value.size() - 1; index1++)
            {
                for (int index2 = index1 + 1; index2 < value.size(); index2++)
                {
                    final Polygon polygon1 = value.get(index1);
                    final Polygon polygon2 = value.get(index2);
                    final org.locationtech.jts.geom.Polygon jtsPolygon1 = JTS_POLYGON_CONVERTER
                            .convert(polygon1);
                    final org.locationtech.jts.geom.Polygon jtsPolygon2 = JTS_POLYGON_CONVERTER
                            .convert(polygon2);
                    // Inner polygons are unioned and and their areas are compared to the sum of the
                    // un-unioned polygons. The areas should be the same (accounting for rounding
                    // errors) if the polygons don't overlap but can touch.
                    if (Math.abs(jtsPolygon1.union(jtsPolygon2).getArea()
                            - (jtsPolygon1.getArea() + jtsPolygon2.getArea())) > 1.0
                                    / Angle.DM7_PER_DEGREE)
                    {
                        problematicPolygons.add(Tuple.createTuple(polygon1, polygon2));
                    }
                }
            }
            // Check that no inner intersects its outer
            value.stream().filter(polygon -> polygon.intersects(key))
                    .forEach(polygon -> problematicPolygons.add(Tuple.createTuple(polygon, key)));
        });

        return problematicPolygons;
    }

    /**
     * Check that outer polygons do not overlap unless one is contained by the inner of the outer
     * that overlaps it.
     *
     * @param outerToInners
     *            {@link MultiMap} of outer {@link Polygon}s to inner {@link Polygon}s
     * @return a {@link Set} of {@link Tuple}s containing {@link Polygon}s that invalidly overlap
     */
    private Set<Tuple<Polygon, Polygon>> checkOuterOverlap(
            final MultiMap<Polygon, Polygon> outerToInners)
    {
        final Set<Tuple<Polygon, Polygon>> problematicPolygons = new HashSet<>();

        final List<Polygon> outersList = new ArrayList<>(outerToInners.keySet());
        // Loop through each combination of polygons only once
        for (int index1 = 0; index1 < outersList.size() - 1; index1++)
        {
            for (int index2 = index1 + 1; index2 < outersList.size(); index2++)
            {
                final Polygon polygon1 = outersList.get(index1);
                final Polygon polygon2 = outersList.get(index2);
                if (polygon1.overlaps(polygon2)
                        // An outer can only be contained by another outer if it is also contained
                        // by one of the other outer's inners
                        && !((polygon1.fullyGeometricallyEncloses(polygon2)
                                && outerToInners.get(polygon1).stream().anyMatch(
                                        inner -> inner.fullyGeometricallyEncloses(polygon2)))
                                || (polygon2.fullyGeometricallyEncloses(polygon1) && outerToInners
                                        .get(polygon2).stream().anyMatch(inner -> inner
                                                .fullyGeometricallyEncloses(polygon1)))))
                {
                    problematicPolygons.add(Tuple.createTuple(polygon1, polygon2));
                }
            }
        }

        return problematicPolygons;
    }

    /**
     * Check that the sub polygons of a {@link MultiPolygon} do not invalidly overlap.
     *
     * @param multiPolygon
     *            {@link MultiPolygon} to check
     * @param osmIdentifier
     *            {@link Long} OSM identifier for the MultiPolygon, to be used in generating
     *            instructions
     * @return a {@link Tuple} containing a {@link Set} of {@link String} instructions for invalid
     *         geometries and a {@link Set} of {@link Location}s marking the invalid geometries
     */
    private Tuple<Set<String>, Set<Location>> checkOverlap(final MultiPolygon multiPolygon,
            final Long osmIdentifier)
    {
        final Set<Tuple<Polygon, Polygon>> problematicPolygons = new HashSet<>();

        final MultiMap<Polygon, Polygon> outerToInners = multiPolygon.getOuterToInners();
        problematicPolygons.addAll(this.checkOuterOverlap(outerToInners));
        problematicPolygons.addAll(this.checkInnerOverlap(outerToInners));

        final Set<String> instructions = new HashSet<>();
        final Set<Location> locations = new HashSet<>();
        problematicPolygons.forEach(tuple ->
        {
            final Location firstCentroid = tuple.getFirst().center();
            final Location secondCentroid = tuple.getFirst().center();
            instructions.add(this.getLocalizedInstruction(INVALID_OVERLAP_INSTRUCTION_FORMAT_INDEX,
                    osmIdentifier, firstCentroid.toCompactString(),
                    secondCentroid.toCompactString()));
            locations.add(firstCentroid);
            locations.add(secondCentroid);
        });
        return Tuple.createTuple(instructions, locations);
    }

    /**
     * Check that a multipolygon {@link Relation} only contains Ways with roles 'outer' or 'inner'.
     *
     * @param multipolygonRelation
     *            {@link Relation} of type multipolygon
     * @return a {@link Set} of {@link String} instructions for invalid members
     */
    private Set<String> checkRolesAndTypes(final Relation multipolygonRelation)
    {
        final Set<String> instructions = new HashSet<>();
        final LinkedHashSet<Long> invalidRoleIDs = new LinkedHashSet<>();
        final LinkedHashSet<Tuple<String, Long>> invalidTypeIDs = new LinkedHashSet<>();

        // Go through each member
        for (final RelationMember relationMember : multipolygonRelation.members())
        {
            // Check that each member is a Way
            if (!atlasToOsmType.get(relationMember.getEntity().getType()).equals("way"))
            {
                invalidTypeIDs.add(
                        Tuple.createTuple(atlasToOsmType.get(relationMember.getEntity().getType()),
                                relationMember.getEntity().getOsmIdentifier()));
            }
            // Check that each member has role outer or inner
            else if (!relationMember.getRole().equals(RelationTypeTag.MULTIPOLYGON_ROLE_OUTER)
                    && !relationMember.getRole().equals(RelationTypeTag.MULTIPOLYGON_ROLE_INNER))
            {
                invalidRoleIDs.add(relationMember.getEntity().getOsmIdentifier());
            }
        }

        // Add instructions for invalid members
        if (!invalidRoleIDs.isEmpty())
        {
            instructions.add(this.getLocalizedInstruction(INVALID_ROLE_INSTRUCTION_FORMAT_INDEX,
                    invalidRoleIDs.size(), multipolygonRelation.getOsmIdentifier(),
                    invalidRoleIDs));
        }
        if (!invalidTypeIDs.isEmpty())
        {
            instructions.add(this.getLocalizedInstruction(INVALID_OSM_TYPE_INSTRUCTION_FORMAT_INDEX,
                    invalidTypeIDs.size(), multipolygonRelation.getOsmIdentifier(),
                    invalidTypeIDs));
        }

        return instructions;
    }

    private Stream<Line> filterMembers(final Relation relation, final Location location)
    {
        return relation.members().stream().map(RelationMember::getEntity)
                .filter(entity -> entity instanceof Line).map(entity -> (Line) entity)
                .filter(line -> line.asPolyLine().contains(location));
    }
}
