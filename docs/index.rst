AutoMapper
==========

A convention-based object-object mapper.

AutoMapper uses a fluent configuration API to define an object-object
mapping strategy. AutoMapper uses a convention-based matching algorithm
to match up source to destination values. AutoMapper is geared towards
model projection scenarios to flatten complex object models to DTOs and
other simple objects, whose design is better suited for serialization,
communication, messaging, or simply an anti-corruption layer between the
domain and application layer.

AutoMapper supports the following platforms:

* .NET 4.5
* .NET Platform Standard 1.1 & 1.3

New to AutoMapper? Check out the :doc:`Getting-started` page first.

.. _user-docs:

.. toctree::
   :hidden:
   :maxdepth: 2
   :caption: User Documentation

   Getting-started
   5.0-Upgrade-Guide
   Conventions

.. _feature-docs:

.. toctree::
   :maxdepth: 2
   :caption: Feature Documentation

   Flattening
   Reverse-Mapping-and-Unflattening
   Projection
   Configuration-validation
   Lists-and-arrays
   Nested-mappings
   Custom-type-converters
   Custom-value-resolvers
   Null-substitution
   Before-and-after-map-actions
   Dependency-injection
   Mapping-inheritance
   Queryable-Extensions
   Configuration
   Conditional-mapping
   Open-Generics
   Understanding-your-mapping

Samples
=======

The source code contains unit tests and samples for all of the features listed above. To view the samples, browse the `source code <https://github.com/AutoMapper/AutoMapper/tree/master/src/AutoMapperSamples>`_.

Housekeeping
============

The latest builds can be found at `NuGet <http://www.nuget.org/packages/automapper>`_

The dev builds can be found at `MyGet <https://www.myget.org/F/automapperdev/api/v2>`_

The discussion group is hosted on `Google Groups <http://groups.google.com/group/automapper-users>`_
