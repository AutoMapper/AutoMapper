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

* `.NET Standard 2.1+ <https://docs.microsoft.com/en-us/dotnet/standard/net-standard>`_

New to AutoMapper? Check out the :doc:`Getting-started` page first.

.. _user-docs:

.. toctree::
   :maxdepth: 2
   :caption: Overview
   
   Getting-started
   Quickstart
   Understanding-your-mapping
   The-MyGet-build

.. _feature-docs:

.. toctree::
   :maxdepth: 2
   :caption: Features
   
   Configuration
   Configuration-validation
   Dependency-injection
   Projection
   Nested-mappings
   Lists-and-arrays
   Construction
   Flattening
   Reverse-Mapping-and-Unflattening
   Mapping-inheritance
   Attribute-mapping
   Dynamic-and-ExpandoObject-Mapping
   Open-Generics
   Queryable-Extensions
   Expression-Translation-(UseAsDataSource)
   Enum-Mapping

.. _Extensibility:

.. toctree::
   :maxdepth: 2
   :caption: Extensibility
   
   Custom-type-converters
   Custom-value-resolvers
   Conditional-mapping
   Null-substitution
   Value-converters
   Value-transformers
   Before-and-after-map-actions

.. _Upgrading:

.. toctree::
   :maxdepth: 2
   :caption: Upgrading
   
   API-Changes
   11.0-Upgrade-Guide
   10.0-Upgrade-Guide
   9.0-Upgrade-Guide
   8.1.1-Upgrade-Guide
   8.0-Upgrade-Guide
   5.0-Upgrade-Guide
      
Examples
========

The source code contains unit tests for all of the features listed above. Use the GitHub search to find relevant examples.

Housekeeping
============

The latest builds can be found at `NuGet <http://www.nuget.org/packages/automapper>`_

The dev builds can be found at `MyGet <https://myget.org/feed/automapperdev/package/nuget/AutoMapper>`_

The discussion group is hosted on `Google Groups <http://groups.google.com/group/automapper-users>`_
