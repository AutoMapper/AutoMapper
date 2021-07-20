package org.openstreetmap.atlas.checks.flag;

import java.io.BufferedWriter;
import java.io.OutputStreamWriter;
import java.io.Serializable;
import java.nio.charset.StandardCharsets;
import java.util.ArrayList;
import java.util.Collection;
import java.util.Collections;
import java.util.HashMap;
import java.util.HashSet;
import java.util.Iterator;
import java.util.LinkedHashSet;
import java.util.List;
import java.util.Objects;
import java.util.Optional;
import java.util.Set;
import java.util.stream.Collectors;

import org.apache.commons.lang3.StringUtils;
import org.openstreetmap.atlas.checks.base.Check;
import org.openstreetmap.atlas.checks.maproulette.data.Task;
import org.openstreetmap.atlas.exception.CoreException;
import org.openstreetmap.atlas.geography.Located;
import org.openstreetmap.atlas.geography.Location;
import org.openstreetmap.atlas.geography.PolyLine;
import org.openstreetmap.atlas.geography.Rectangle;
import org.openstreetmap.atlas.geography.atlas.change.FeatureChange;
import org.openstreetmap.atlas.geography.atlas.items.AtlasItem;
import org.openstreetmap.atlas.geography.atlas.items.AtlasObject;
import org.openstreetmap.atlas.geography.atlas.items.LocationItem;
import org.openstreetmap.atlas.geography.atlas.items.Relation;
import org.openstreetmap.atlas.geography.geojson.GeoJsonBuilder;
import org.openstreetmap.atlas.geography.geojson.GeoJsonBuilder.GeometryWithProperties;
import org.openstreetmap.atlas.geography.geojson.GeoJsonType;
import org.openstreetmap.atlas.geography.geojson.GeoJsonUtils;
import org.openstreetmap.atlas.streaming.resource.WritableResource;
import org.openstreetmap.atlas.utilities.collections.Iterables;
import org.openstreetmap.atlas.utilities.collections.MultiIterable;
import org.openstreetmap.atlas.utilities.scalars.Distance;
import org.slf4j.Logger;
import org.slf4j.LoggerFactory;

import com.google.gson.JsonArray;
import com.google.gson.JsonObject;

/**
 * A {@link CheckFlag} is used to flag one or more {@link AtlasObject}s found to violate some set of
 * rules.
 *
 * @author matthieun
 * @author cuthbertm
 * @author mgostintsev
 * @author brian_l_davis
 * @author bbreithaupt
 */
public class CheckFlag implements Iterable<Location>, Located, Serializable
{
    public static final String NULL_IDENTIFIERS = "nullnull";
    private static final Distance TEN_METERS = Distance.meters(10);
    private static final Logger logger = LoggerFactory.getLogger(CheckFlag.class);
    private static final long serialVersionUID = -1287808902452203852L;
    private String challengeName = null;
    private Set<FlaggedObject> flaggedObjects = new LinkedHashSet<>();
    private final String identifier;
    private final List<String> instructions = new ArrayList<>();
    private final Set<FeatureChange> fixSuggestions = new HashSet<>();

    /**
     * A basic constructor that simply flags some identifying value
     *
     * @param identifier
     *            the identifying value to flag
     */
    public CheckFlag(final String identifier)
    {
        this.identifier = identifier;
    }

    /**
     * Creates a {@link CheckFlag} with a set of {@link AtlasObject}s to flag, most commonly used
     * when all of the {@link AtlasObject}s contribute to the rule violation
     *
     * @param identifier
     *            the identifying value to flag
     * @param objects
     *            {@link AtlasObject}s to flag
     * @param instructions
     *            a list of free form instructions
     */
    public CheckFlag(final String identifier, final Set<? extends AtlasObject> objects,
            final List<String> instructions)
    {
        this(identifier, objects, instructions, new ArrayList<>());
    }

    /**
     * Creates a {@link CheckFlag} with the addition of a list of {@code point} {@link Location}s
     * that highlight specific points in the geometry that caused the rule violation
     *
     * @param identifier
     *            the identifying value to flag
     * @param objects
     *            {@link AtlasObject}s to flag
     * @param instructions
     *            a list of free form instructions
     * @param points
     *            {@code point} {@link Location}s to highlight
     */
    public CheckFlag(final String identifier, final Set<? extends AtlasObject> objects,
            final List<String> instructions, final List<Location> points)
    {
        this(identifier, objects, instructions, points, new HashSet<>());
    }

    /**
     * Creates a {@link CheckFlag} with the addition of a list of {@code point} {@link Location}s
     * that highlight specific points and a {@link Set} of {@link FeatureChange}s that suggest how
     * to fix the flagged objects.
     *
     * @param identifier
     *            the identifying value to flag
     * @param objects
     *            {@link AtlasObject}s to flag
     * @param instructions
     *            a list of free form instructions
     * @param points
     *            {@code point} {@link Location}s to highlight
     * @param fixSuggestions
     *            {@link Set} of {@link FeatureChange}s representing suggested fixes for flagged
     *            features
     */
    public CheckFlag(final String identifier, final Set<? extends AtlasObject> objects,
            final List<String> instructions, final List<Location> points,
            final Set<FeatureChange> fixSuggestions)
    {
        this.addObjects(objects);
        this.addPoints(points);
        this.addInstructions(instructions);
        this.identifier = identifier;
        this.addFixSuggestions(fixSuggestions);
    }

    /**
     * Add a single {@link FeatureChange} to the fix suggestions. Fix suggestions should be
     * {@link FeatureChange}s of
     * {@link org.openstreetmap.atlas.geography.atlas.complete.CompleteEntity}s created from flagged
     * {@link org.openstreetmap.atlas.geography.atlas.items.AtlasEntity}s with changes suggesting
     * how the flagged feature can be fixed.
     *
     * @param suggestion
     *            {@link FeatureChange} with suggested alterations to a flagged feature
     * @return this {@link CheckFlag}
     */
    public CheckFlag addFixSuggestion(final FeatureChange suggestion)
    {
        this.fixSuggestions.add(suggestion);
        return this;
    }

    /**
     * Add a {@link Set} of {@link FeatureChange}s to the fix suggestions.
     *
     * @param suggestions
     *            {@link Collection} of {@link FeatureChange} fix suggestions
     * @return this {@link CheckFlag}
     */
    public CheckFlag addFixSuggestions(final Collection<FeatureChange> suggestions)
    {
        this.fixSuggestions.addAll(suggestions);
        return this;
    }

    /**
     * Adds any instructions that may help communicate why the {@link AtlasObject}(s) were flagged
     *
     * @param instruction
     *            a free form instruction
     */
    public void addInstruction(final String instruction)
    {
        if (StringUtils.isNotEmpty(instruction))
        {
            this.instructions.add(instruction);
        }
    }

    /**
     * Adds a list of instructions that may help communicate why the {@link AtlasObject}(s) were
     * flagged. This can be useful if multiple rules were violated
     *
     * @param instructions
     *            a list of free form instruction
     */
    public void addInstructions(final Iterable<String> instructions)
    {
        instructions.forEach(this::addInstruction);
    }

    /**
     * Adds an {@link AtlasObject} to flag
     *
     * @param object
     *            an {@link AtlasObject}
     */
    public void addObject(final AtlasObject object)
    {
        if (object instanceof AtlasItem)
        {
            if (object instanceof LocationItem)
            {
                this.flaggedObjects.add(new FlaggedPoint((LocationItem) object));
            }
            else
            {
                this.flaggedObjects.add(new FlaggedPolyline((AtlasItem) object));
            }
        }

        // If object is instance of relation, then add the relation to flaggedRelations set
        else if (object instanceof Relation)
        {
            this.flaggedObjects.add(new FlaggedRelation((Relation) object));
        }
    }

    /**
     * Adds a {@link FlaggedObject} to the flag.
     *
     * @param object
     *            {@link FlaggedObject}
     */
    public void addObject(final FlaggedObject object)
    {
        this.flaggedObjects.add(object);
    }

    /**
     * Flags an {@link AtlasObject}, highlighting a specific {@code point} {@link Location} and
     * instructions with more detail. This helps build flags iteratively when more complex
     * {@link Check}s span a large number of {@link AtlasObject}s.
     *
     * @param object
     *            the {@link AtlasObject} to flag
     * @param point
     *            the {@code point} {@link Location} to highlight
     * @param instruction
     *            a free form instruction
     */
    public void addObject(final AtlasObject object, final Location point, final String instruction)
    {
        this.addObject(object);
        this.addPoint(point);
        this.addInstruction(instruction);
    }

    /**
     * Flags an {@link AtlasObject} with instructions. This helps build flags iteratively when more
     * complex {@link Check}s span a large number of {@link AtlasObject}s
     *
     * @param object
     *            the {@link AtlasObject} to flag
     * @param instruction
     *            a free form instruction
     */
    public void addObject(final AtlasObject object, final String instruction)
    {
        this.addObject(object);
        this.addInstruction(instruction);
    }

    /**
     * Adds a list of {@link AtlasObject}s to flag
     *
     * @param objects
     *            a list of {@link AtlasObject}s
     */
    public void addObjects(final Iterable<? extends AtlasObject> objects)
    {
        objects.forEach(this::addObject);
    }

    /**
     * Flags a specific {@link Location}
     *
     * @param point
     *            the {@code point} {@link Location} to flag
     */
    public void addPoint(final Location point)
    {
        this.flaggedObjects.add(new FlaggedPoint(point));
    }

    /**
     * Flags a list of {@code point} {@link Location}s
     *
     * @param points
     *            the {@code point} {@link Location}s to flag
     */
    public void addPoints(final Iterable<Location> points)
    {
        Iterables.stream(points).map(FlaggedPoint::new).forEach(this.flaggedObjects::add);
    }

    public JsonObject asGeoJsonFeature()
    {
        final JsonObject geometry = this.boundsGeoJsonGeometry();

        final JsonObject properties = new JsonObject();
        properties.addProperty("flag:type", CheckFlag.class.getSimpleName());
        properties.addProperty("flag:id", this.getIdentifier());
        properties.addProperty("flag:instructions", this.getInstructions());

        // The legacy GeoJSON FeatureCollection doesn't actually provide this,
        // but I figure this might be useful to know about if it's there...
        if (this.challengeName != null)
        {
            properties.addProperty("flag:challenge", this.challengeName);
        }

        return GeoJsonUtils.feature(geometry, properties);
    }

    @Override
    public Rectangle bounds()
    {
        return Rectangle.forLocated(new MultiIterable<>(this.getShapes()));
    }

    @Override
    public boolean equals(final Object other)
    {
        if (this == other)
        {
            return true;
        }

        if (!(other instanceof CheckFlag))
        {
            return false;
        }

        final CheckFlag otherFlag = (CheckFlag) other;
        return Objects.equals(this.identifier, otherFlag.identifier)
                && Objects.equals(this.challengeName, otherFlag.challengeName)
                && Objects.equals(this.instructions, otherFlag.instructions)
                && Objects.equals(this.flaggedObjects, otherFlag.flaggedObjects)
                && Objects.equals(this.fixSuggestions, otherFlag.fixSuggestions);
    }

    /**
     * @return a Challenge name
     */
    public Optional<String> getChallengeName()
    {
        return Optional.ofNullable(this.challengeName);
    }

    /**
     * Will return the first country ISO that it finds in the objects provided in the flag
     *
     * @return a country ISO3 code
     */
    public String getCountryISO()
    {
        for (final FlaggedObject object : this.flaggedObjects)
        {
            if (object.hasCountry())
            {
                return object.getCountry();
            }
        }
        return FlaggedObject.COUNTRY_MISSING;
    }

    /**
     * @return a {@link Set} of {@link FeatureChange} fix suggestions
     */
    public Set<FeatureChange> getFixSuggestions()
    {
        return this.fixSuggestions;
    }

    /**
     * @return a set of flagged {@link AtlasObject}s
     */
    public Set<FlaggedObject> getFlaggedObjects()
    {
        return this.flaggedObjects;
    }

    /**
     * @return a set of flagged {@link Relation}s
     */
    public Set<FlaggedObject> getFlaggedRelations()
    {
        return this.flaggedObjects.stream().filter(FlaggedRelation.class::isInstance)
                .collect(Collectors.toSet());
    }

    /**
     * @return a list of {@link GeometryWithProperties} representing all flagged geometries
     */
    public List<GeometryWithProperties> getGeometryWithProperties()
    {
        return this.flaggedObjects.stream()
                .filter(flaggedObject -> flaggedObject instanceof FlaggedPoint
                        || flaggedObject instanceof FlaggedPolyline)
                .map(flaggedObject -> new GeometryWithProperties(flaggedObject.getGeometry(),
                        new HashMap<>(flaggedObject.getProperties())))
                .collect(Collectors.toList());
    }

    /**
     * @return the flag identifier
     */
    public String getIdentifier()
    {
        return this.identifier;
    }

    /**
     * @return all of the instructions in a {@code String}
     */
    public String getInstructions()
    {
        final StringBuilder builder = new StringBuilder();
        int instructionNumber = 1;
        for (final String instruction : this.instructions)
        {
            if (StringUtils.isNotEmpty(instruction))
            {
                if (builder.length() > 0)
                {
                    builder.append("\n");
                }
                // This is a visually displayed list of instructions for use in Map Roulette
                builder.append(instructionNumber++ + ". " + instruction);
            }
        }
        return builder.toString();
    }

    /**
     * Builds a MapRouletted {@link Task} from this {@link CheckFlag}
     *
     * @return a {@link Task}
     */
    public Task getMapRouletteTask()
    {
        final Task task = new Task();
        task.setInstruction(this.getInstructions());
        task.setProjectName(this.getCountryISO());
        task.setChallengeName(this.getChallengeName().orElse(this.getClass().getSimpleName()));
        task.setTaskIdentifier(this.identifier);

        // Add custom pin point(s), if supplied.
        final Set<Location> points = this.getPoints();
        if (!points.isEmpty())
        {
            task.setPoints(points);
        }
        else
        {
            final Set<PolyLine> polyLines = this.getPolyLines();
            if (!polyLines.isEmpty())
            {
                // Retrieve the first item in the list and retrieve the first point in the
                // geometry for the object
                task.setPoint(polyLines.iterator().next().iterator().next());
            }
        }

        final JsonArray features = new JsonArray();
        // Features
        if (!this.getGeometryWithProperties().isEmpty())
        {
            this.getGeometryWithProperties()
                    .forEach(shape -> features.add(new GeoJsonBuilder().create(shape)));
        }
        final Set<FlaggedObject> flaggedRelations = this.getFlaggedRelations();
        if (!flaggedRelations.isEmpty())
        {
            this.getFlaggedRelations().stream()
                    .map(flaggedRelation -> flaggedRelation.asGeoJsonFeature(this.identifier))
                    .forEach(features::add);
        }
        task.setGeoJson(Optional.of(features));
        return task;
    }

    /**
     * Wraps getMapRouletteTask() to embed optional fix suggestions
     * 
     * @param includeFixSuggestions
     *            true if we want to upload fix suggestions, false if not
     * @return a {@link Task}
     */
    public Task getMapRouletteTask(final boolean includeFixSuggestions)
    {
        final Task returnTask = this.getMapRouletteTask();
        if (includeFixSuggestions && !this.fixSuggestions.isEmpty())
        {
            returnTask.setCooperativeWork(this.fixSuggestions);
        }
        return returnTask;
    }

    /**
     * @return a set of all {@code point} {@link Location} geometries flagged
     */
    public Set<Location> getPoints()
    {
        return this.flaggedObjects.stream().map(FlaggedObject::getGeometry)
                .filter(geometry -> geometry instanceof Location)
                .map(geometry -> (Location) geometry).collect(Collectors.toSet());
    }

    /**
     * @return a set of all {@code polyline} geometries flagged
     */
    public Set<PolyLine> getPolyLines()
    {
        return this.flaggedObjects.stream().map(FlaggedObject::getGeometry)
                .filter(geometry -> geometry instanceof PolyLine)
                .map(geometry -> (PolyLine) geometry).collect(Collectors.toSet());
    }

    public List<String> getRawInstructions()
    {
        return this.instructions;
    }

    /**
     * @return a {@code shape} representation of all {@code polyline} geometries flagged
     */
    public Iterable<Iterable<Location>> getShapes()
    {
        return Iterables.asIterable(this.getPolyLines().stream()
                .map(polyLine -> (Iterable<Location>) polyLine).collect(Collectors.toList()));
    }

    /**
     * Generates an id {@link Set} for unique flag identification. The set is comprised of the item
     * type + atlas id of the flagged objects. If there are no objects with atlas ids then the set
     * only contains the check flag id.
     *
     * @return a {@link Set} of the unique ids
     */
    public Set<String> getUniqueIdentifiers()
    {
        final Set<String> flaggedObjectIdentifiers = this.flaggedObjects.stream()
                .map(FlaggedObject::getUniqueIdentifier)
                .filter(string -> !string.equals(NULL_IDENTIFIERS)).collect(Collectors.toSet());
        return flaggedObjectIdentifiers.isEmpty() ? Collections.singleton(this.identifier)
                : flaggedObjectIdentifiers;
    }

    @Override
    public int hashCode()
    {
        return Objects.hash(this.identifier, this.challengeName, this.instructions,
                this.flaggedObjects);
    }

    @Override
    public Iterator<Location> iterator()
    {
        return new MultiIterable<>(this.getShapes()).iterator();
    }

    /**
     * Decouple the {@link CheckFlag} from any
     * {@link org.openstreetmap.atlas.geography.atlas.Atlas}s by making all the
     * {@link FlaggedObject}s complete.
     *
     * @return this
     */
    public CheckFlag makeComplete()
    {
        final LinkedHashSet<FlaggedObject> completeFlaggedObjects = new LinkedHashSet<>();
        this.flaggedObjects.forEach(flaggedObject -> completeFlaggedObjects
                .add(flaggedObject.getAsCompleteFlaggedObject()));
        this.flaggedObjects = completeFlaggedObjects;
        return this;
    }

    /**
     * Writes the string value of this {@link CheckFlag} to the {@link WritableResource}
     *
     * @param writableResource
     *            a {@link WritableResource} to write to
     */
    public void save(final WritableResource writableResource)
    {
        try (BufferedWriter out = new BufferedWriter(
                new OutputStreamWriter(writableResource.write(), StandardCharsets.UTF_8)))
        {
            out.write(this.toString());
        }
        catch (final Exception e)
        {
            throw new CoreException("Could not save Check Flag to {}", e, writableResource);
        }
    }

    /**
     * Sets a Challenge name for this Flag
     *
     * @param challengeName
     *            a Challenge name
     */
    public void setChallengeName(final String challengeName)
    {
        this.challengeName = challengeName;
    }

    @Override
    public String toString()
    {
        return String.format("[CheckFlag: %s, %s]", this.identifier, this.getInstructions());
    }

    private JsonObject boundsGeoJsonGeometry()
    {
        final Iterator<FlaggedObject> iterator = this.flaggedObjects.iterator();
        Rectangle bounds;

        // Get the first bounds.
        if (iterator.hasNext())
        {
            bounds = iterator.next().bounds();
        }
        // If we don't have it, let's instead return null island.
        else
        {
            return GeoJsonUtils.geometry(GeoJsonType.POINT, GeoJsonUtils.coordinate(0.0, 0.0));
        }

        // Otherwise, let's get the rest of the bounds and expand the bounds we have.
        while (iterator.hasNext())
        {
            final Rectangle nextBounds = iterator.next().bounds();
            bounds = bounds.combine(nextBounds);
        }

        // We want the bbox to be at least ten meters wide and high. This is for straight lines and
        // single point flags. I figure this is a good minimum, as it's about the width of a tennis
        // court, and that seems like a good minimum unit to browse a check flag on the map.
        if (bounds.width().onEarth().isLessThan(TEN_METERS))
        {
            bounds = bounds.expandHorizontally(TEN_METERS);
        }

        if (bounds.height().onEarth().isLessThan(TEN_METERS))
        {
            bounds = bounds.expandVertically(TEN_METERS);
        }

        // Turn that bounds into a GeoJSON geometry.
        return GeoJsonUtils.boundsToPolygonGeometry(bounds);
    }
}
