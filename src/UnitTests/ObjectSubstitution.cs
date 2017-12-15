namespace AutoMapper.UnitTests
{
    namespace ObjectSubstitution
    {
        using System;
        using Shouldly;
        using Xunit;

        public abstract class Animal
        {
            public int key;
        }
        public class Cat : Animal { }
        public class Dog : Animal { }

        public class AnimalProxy : Animal
        {
            public Type ToConvert { get; set; }
        }

        public class CatProxy : Cat
        {
            public Type ToConvert { get; set; }

        }
        public class DogProxy : Dog { }

        public abstract class AnimalDto
        {
            public int key;
        }
        public class CatDto : AnimalDto { }
        public class DogDto : AnimalDto { }


        public class OverrideExample : AutoMapperSpecBase
        {
            private AnimalDto _animalDto;

            protected override MapperConfiguration Configuration { get; } = new MapperConfiguration(cfg =>
            {
                cfg.CreateMap<Animal, AnimalDto>()
                    .Substitute(CastToEntity).AfterMap(TransformDestination)
                    .Include<Cat, CatDto>()
                    .Include<Dog, DogDto>();
                cfg.CreateMap<Cat, CatDto>();
                cfg.CreateMap<Dog, DogDto>();
            });

            private static void TransformDestination(Animal arg1, AnimalDto arg2)
            {
                arg2.key = 0;
            }

            protected override void Because_of()
            {
                var proxy = new CatProxy
                {
                    key = 1,
                    ToConvert = typeof(Cat)
                };
                _animalDto = Mapper.Map<Animal, AnimalDto>(proxy);
            }

            [Fact]
            public void Should_substitute_correct_object()
            {
                _animalDto.ShouldBeOfType<CatDto>();
                Assert.Equal(0, _animalDto.key);
            }

            private static object CastToEntity(Animal entity)
            {
                return Activator.CreateInstance(((CatProxy)entity).ToConvert);
            }
        }
    }
}