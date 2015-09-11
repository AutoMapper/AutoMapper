using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using AutoMapperSamples.EF.Dtos;
using AutoMapperSamples.EF.Model;
using NUnit.Core;
using NUnit.Framework;

namespace AutoMapperSamples.EF
{
    [TestFixture]
    public class PrerequisiteTests
    {
        private List<Exception> exceptions;
        private Master master;
        private Detail detail;

        [SetUp]
        public void SetUp()
        {
            exceptions = new List<Exception>();
            Mapper.Initialize(cfg =>
            {
                cfg.CreateMap<Master, MasterDto>();
                cfg.CreateMap<MasterDto, Master>();
                cfg.CreateMap<Detail, DetailDto>();
                cfg.CreateMap<DetailDto, Detail>();
            });

            master = new Master()
            {
                Name = "Harry Marry",
                Id = Guid.NewGuid(),
            };
            detail = new Detail()
            {
                Id = Guid.NewGuid(),
                Name = "Test Order",
                Master = master,
            };
            master.Details.Add(detail);
        }

        [Test]
        public void CanMapCyclicObjectGraph()
        {
            // Arrange

            // Act
            var dto = Mapper.Map<DetailDto>(detail);

            // Assert
            AssertValidDtoGraph(dto);
        }

        [Test]
        public void CanMapCaclicExpressionGraph()
        {
            // Arrange
            var detailQuery = new List<Detail> { detail }.AsQueryable();

            // Act
            var detailDtoQuery = detailQuery.UseAsDataSource()
                .OnError(ex => exceptions.Add(ex))
                .For<DetailDto>();

            // Assert
            var dto = detailDtoQuery.Single();

            AssertValidDtoGraph(dto);
        }

        [Test]
        public void CanMapCaclicExpressionGraph_WithPropertyFilter()
        {
            // Arrange
            var detailQuery = new List<Detail> { detail }.AsQueryable();

            // Act
            var detailDtoQuery = detailQuery.UseAsDataSource()
                .OnError(ex => exceptions.Add(ex))
                .For<DetailDto>()
                .Where(d => d.Name.EndsWith("rder"));

            // Assert
            var dto = detailDtoQuery.Single();

            AssertValidDtoGraph(dto);
        }

        [Test]
        public void CanMapCaclicExpressionGraph_WithPropertyPathEqualityFilter_Single()
        {
            // Arrange
            var detailQuery = new List<Detail> { detail }.AsQueryable();

            // Act
            var detailDtoQuery = detailQuery.UseAsDataSource()
                .OnError(ex => exceptions.Add(ex))
                .For<DetailDto>()
                .Where(d => d.Master.Name == "Harry Marry");

            // Assert
            var dto = detailDtoQuery.Single();

            AssertValidDtoGraph(dto);
        }

        [Test]
        public void CanMapCaclicExpressionGraph_WithPropertyPathEqualityFilter_ToList()
        {
            // Arrange
            var detailQuery = new List<Detail> { detail }.AsQueryable();

            // Act
            var detailDtoQuery = detailQuery.UseAsDataSource()
                .OnError(ex => exceptions.Add(ex))
                .For<DetailDto>()
                .Where(d => d.Master.Name == "Harry Marry");

            // Assert
            var dto = detailDtoQuery.ToList();

            AssertValidDtoGraph(dto.Single());
        }
        
        [Test]
        public void CanMapCaclicExpressionGraph_WithoutResults_Single()
        {
            // Arrange
            var detailQuery = new List<Detail> {  }.AsQueryable();

            // Act
            var detailDtoQuery = detailQuery.UseAsDataSource()
                .OnError(ex => exceptions.Add(ex))
                .For<DetailDto>()
                .Where(d => d.Master.Name == "Harry Marry");

            // Assert
            var dto = detailDtoQuery.SingleOrDefault();
            
            Assert.IsNull(dto);
        }
        
        [Test]
        public void CanMapCaclicExpressionGraph_WithoutResults_ToList()
        {
            // Arrange
            var detailQuery = new List<Detail> { }.AsQueryable();

            // Act
            var detailDtoQuery = detailQuery.UseAsDataSource()
                .OnError(ex => exceptions.Add(ex))
                .For<DetailDto>()
                .Where(d => d.Master.Name == "Harry Marry");

            // Assert
            var dto = detailDtoQuery.ToList();

            Assert.AreEqual(0, dto.Count);
        }

        [Test]
        public void CanMapCaclicExpressionGraph_WithPropertyPathMethodCallFilter()
        {
            // Arrange
            var detailQuery = new List<Detail> {detail}.AsQueryable();

            // Act
            var detailDtoQuery = detailQuery.UseAsDataSource()
                .OnError(ex => exceptions.Add(ex))
                .For<DetailDto>()
                .Where(d => d.Master.Name.EndsWith("Marry"));

            // Assert
            var dto = detailDtoQuery.Single();
            
            AssertValidDtoGraph(dto);
        }

        private void AssertValidDtoGraph(DetailDto dto)
        {
            Assert.IsNotNull(dto);
            Assert.AreEqual(detail.Id, dto.Id);
            Assert.IsNotNull(detail.Master);
            Assert.IsNotEmpty(master.Details);
            Assert.AreEqual(detail.Master.Id, master.Id);
            Assert.AreSame(dto.Master.Details.Single(), dto);
        }

        #region Entities

        private class Master
        {
            public Master()
            {
                Details = new HashSet<Detail>();
            }
            public Guid Id { get; set; }
            public string Name { get; set; }
            public ICollection<Detail> Details { get; set; }
        }

        private class Detail
        {
            public Guid Id { get; set; }
            public string Name { get; set; }
            public Master Master { get; set; }
        }

        #endregion

        #region DTOs

        private class MasterDto
        {
            public MasterDto()
            {
                Details = new HashSet<DetailDto>();
            }
            public Guid Id { get; set; }
            public string Name { get; set; }
            public ICollection<DetailDto> Details { get; set; }
        }

        private class DetailDto
        {
            public Guid Id { get; set; }
            public string Name { get; set; }
            public MasterDto Master { get; set; }
        }

        #endregion
    }
}
