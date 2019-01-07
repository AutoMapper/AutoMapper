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

* .NET 4.6.1+
* `.NET Standard 2.0+ https://docs.microsoft.com/en-us/dotnet/standard/net-standard`

New to AutoMapper? Check out the :doc:`Getting-started` page first.

.. _user-docs:

.. toctree::
   :hidden:
   :maxdepth: 2
   :caption: User Documentation

   Getting-started
   5.0-Upgrade-Guide
   8.0-Upgrade-Guide
   Static-and-Instance-API
   Migrating-from-static-API
   Conventions
   Understanding-your-mapping
   The-MyGet-build

.. _feature-docs:

.. toctree::
   :maxdepth: 2
   :caption: Feature Documentation

   Flattening
   Reverse-Mapping-and-Unflattening
   Projection
   Configuration-validation
   Inline-Mapping
   Lists-and-arrays
   Nested-mappings
   Custom-type-converters
   Custom-value-resolvers
   Value-converters
   Value-transformers
   Null-substitution
   Before-and-after-map-actions
   Dependency-injection
   Mapping-inheritance
   Queryable-Extensions
   Configuration
   Construction
   Conditional-mapping
   Open-Generics
   Dynamic-and-ExpandoObject-Mapping
   Expression-Translation-(UseAsDataSource)
      
Examples
========

The source code contains unit tests for all of the features listed above. Use the GitHub search to find relevant examples.

Housekeeping
============

The latest builds can be found at `NuGet <http://www.nuget.org/packages/automapper>`_

The dev builds can be found at `MyGet <https://myget.org/feed/automapperdev/package/nuget/AutoMapper>`_

The discussion group is hosted on `Google Groups <http://groups.google.com/group/automapper-users>`_
