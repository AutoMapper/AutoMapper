namespace AutoMapper.UnitTests
{
    namespace ObjectSubstitution
    {
        using System;
        using Should;
        using Xunit;

        public abstract class Animal { }
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

        public abstract class AnimalDto { }
        public class CatDto : AnimalDto { }
        public class DogDto : AnimalDto { }


        public class OverrideExample : AutoMapperSpecBase
        {
            private AnimalDto _animalDto;

            protected override MapperConfiguration Configuration { get; } = new MapperConfiguration(cfg =>
            {
                cfg.CreateMap<Animal, AnimalDto>()
                    .Substitute(CastToEntity)
                    .Include<Cat, CatDto>()
                    .Include<Dog, DogDto>();
                cfg.CreateMap<Cat, CatDto>();
                cfg.CreateMap<Dog, DogDto>();
            });

            protected override void Because_of()
            {
                var proxy = new CatProxy
                {
                    ToConvert = typeof (Cat)
                };
                _animalDto = Mapper.Map<Animal, AnimalDto>(proxy);
            }

            [Fact]
            public void Should_substitute_correct_object()
            {
                _animalDto.ShouldBeType<CatDto>();
            }

            private static object CastToEntity(Animal entity)
            {
                return Activator.CreateInstance(((CatProxy)entity).ToConvert);
            }
        }
    }
}