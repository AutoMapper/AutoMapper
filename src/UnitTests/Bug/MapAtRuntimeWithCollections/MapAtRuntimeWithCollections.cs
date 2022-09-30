using OmmitedDatabaseModel3WithCollections;
using OmmitedDTOModel3WithCollections;

namespace AutoMapper.UnitTests;

public class MapAtRuntimeWithCollections : AutoMapperSpecBase
{
    protected override MapperConfiguration CreateConfiguration() => new(cfg =>
    {
        cfg.CreateMap<Entity1, EntityDTO1>().ReverseMap();
        cfg.CreateMap<Entity2, EntityDTO2>().ReverseMap();
        cfg.CreateMap<Entity3, EntityDTO3>().ReverseMap();
        cfg.CreateMap<Entity4, EntityDTO4>().ReverseMap();
        cfg.CreateMap<Entity5, EntityDTO5>().ReverseMap();
        cfg.CreateMap<Entity6, EntityDTO6>().ReverseMap();
        cfg.CreateMap<Entity7, EntityDTO7>().ReverseMap();
        cfg.CreateMap<Entity8, EntityDTO8>().ReverseMap();
        cfg.CreateMap<Entity9, EntityDTO9>().ReverseMap();
        cfg.CreateMap<Entity10, EntityDTO10>().ReverseMap();
        cfg.CreateMap<Entity11, EntityDTO11>().ReverseMap();
        cfg.CreateMap<Entity12, EntityDTO12>().ReverseMap();
        cfg.CreateMap<Entity13, EntityDTO13>().ReverseMap();
        cfg.CreateMap<Entity14, EntityDTO14>().ReverseMap();
        cfg.CreateMap<Entity15, EntityDTO15>().ReverseMap();
        cfg.CreateMap<Entity16, EntityDTO16>().ReverseMap();
        cfg.CreateMap<Entity17, EntityDTO17>().ReverseMap();
        cfg.CreateMap<Entity18, EntityDTO18>().ReverseMap();
        cfg.CreateMap<Entity19, EntityDTO19>().ReverseMap();
        cfg.CreateMap<Entity20, EntityDTO20>().ReverseMap();
        cfg.CreateMap<Entity21, EntityDTO21>().ReverseMap();
        cfg.CreateMap<Entity22, EntityDTO22>().ReverseMap();
        cfg.CreateMap<Entity23, EntityDTO23>().ReverseMap();
        cfg.CreateMap<Entity24, EntityDTO24>().ReverseMap();
        cfg.CreateMap<Entity25, EntityDTO25>().ReverseMap();
        cfg.CreateMap<Entity26, EntityDTO26>().ReverseMap();
        //cfg.ForAllPropertyMaps(p => !p.SourceType.IsValueType, (pm, o) => o.MapAtRuntime());
    });

    public class Initialize
    {
        Entity2 appointmentStatusHistory1;
        Entity8 center1;
        Entity12 insurance1;
        Entity14 patient1;
        Entity17 resource1;
        Entity20 service1;
        Entity22 speciality1;

        public Initialize()
        {
            appointmentStatusHistory1 = new Entity2 { Id = Guid.NewGuid() };
            center1 = new Entity8 { Id = Guid.NewGuid() };
            insurance1 = new Entity12 { Id = Guid.NewGuid() };
            patient1 = new Entity14 { Id = Guid.NewGuid() };
            resource1 = new Entity17 { Id = Guid.NewGuid() };
            service1 = new Entity20 { Id = Guid.NewGuid() };
            speciality1 = new Entity22 { Id = Guid.NewGuid() };

            speciality1.Entities20.Add(service1);

            service1.Entities8.Add(center1);
            service1.Entities12.Add(insurance1);
            service1.Entities17.Add(resource1);
            service1.Entity22 = speciality1;
            service1.Entity22Id = speciality1.Id;

            resource1.Entities8.Add(center1);
            resource1.Entities20.Add(service1);

            patient1.Entities12.Add(insurance1);

            insurance1.Entities14.Add(patient1);
            insurance1.Entities20.Add(service1);

            center1.Entities17.Add(resource1);
            center1.Entities20.Add(service1);
            center1.Entities22.Add(speciality1);
        }

        public Entity1 GenerateAppointment()
        {
            var appointment1 = new Entity1 { Id = Guid.NewGuid() };
            appointmentStatusHistory1.Entity1 = appointment1;
            appointmentStatusHistory1.Entity1Id = appointment1.Id;
            appointment1.Entities2.Add(appointmentStatusHistory1);
            appointment1.Entity8 = center1;
            appointment1.Entity8Id = center1.Id;
            appointment1.Entity12 = insurance1;
            appointment1.Entity12Id = insurance1.Id;
            appointment1.Entity14 = patient1;
            appointment1.Entity14Id = patient1.Id;
            appointment1.Entity17 = resource1;
            appointment1.Entity17Id = resource1.Id;
            appointment1.Entity20 = service1;
            appointment1.Entity20Id = service1.Id;
            appointment1.Entity22 = speciality1;
            appointment1.Entity22Id = speciality1.Id;

            patient1.Entities1.Add(appointment1);

            return appointment1;
        }
    }

    [Fact]
    public void ShouldNotBeSlow()
    {
        //List of objects performing slow
        //Entity1
        //Entity17
        //Entity25
        //Entity19
        //Entity15
        //Entity13
        //Entity7
        //Entity5
        //Entity2

        var list = new List<Entity1>();
        var initialize = new Initialize();
        list.Add(initialize.GenerateAppointment());
        var appointmentsDTO = Mapper.Map<List<EntityDTO1>>(list);
        var list2 = new List<Entity1>();
        var entity = new Entity1();
        list2.Add(entity);
        var DTOs = Mapper.Map<List<EntityDTO1>>(list2);
        var list3 = new List<Entity17>();
        var entity17 = new Entity17();
        list3.Add(entity17);
        var DTOs17 = Mapper.Map<List<EntityDTO17>>(list3);
        var list4 = new List<Entity25>();
        var entity25 = new Entity25();
        list4.Add(entity25);
        var DTOs25 = Mapper.Map<List<EntityDTO25>>(list4);
    }
}