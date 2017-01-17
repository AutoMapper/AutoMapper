using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using ContosoUniversity.Data.Enitities;
using ContosoUniversity.Domain.School;

namespace ContosoUniversity.AutoMapperProfiles
{
    public class UniversityProfile : Profile
    {
        public UniversityProfile()
        {
            CreateMap<CourseAssignmentModel, CourseAssignment>().ReverseMap();

            CreateMap<CourseModel, Course>()
                .ReverseMap()
                .ForMember(dest => dest.DepartmentName, opts => opts.MapFrom(x => x.Department.Name)); ;

            CreateMap<DepartmentModel, Department>()
                .ReverseMap()
                //.ForMember(dest => dest.StartDate, opts => opts.MapFrom(x => x.StartDate.Date)) This works in .Net Core
                .ForMember(dest => dest.AdministratorName, opts => opts.MapFrom(x => string.Concat(x.Administrator.FirstMidName, " ", x.Administrator.LastName)));

            CreateMap<EnrollmentModel, Enrollment>()
                .ReverseMap()
                .ForMember(dest => dest.CourseTitle, opts => opts.MapFrom(x => x.Course.Title))
                .ForMember(dest => dest.GradeLetter, opts => opts.MapFrom(x => x.Grade.HasValue ? x.Grade.Value.ToString() : string.Empty));
            //public string GradeLetter { get { return this.Grade.HasValue ? this.Grade.Value.ToString() : string.Empty; } }
            CreateMap<InstructorModel, Instructor>()
                .ForMember(dest => dest.FirstMidName, opts => opts.MapFrom(x => x.FirstName))
                .ReverseMap()
                .ForMember(dest => dest.FirstName, opts => opts.MapFrom(x => x.FirstMidName))
                .ForMember(dest => dest.FullName, opts => opts.MapFrom(x => string.Concat(x.FirstMidName, " ", x.LastName)));

            CreateMap<OfficeAssignmentModel, OfficeAssignment>().ReverseMap();

            CreateMap<StudentModel, Student>()
                .ForMember(dest => dest.FirstMidName, opts => opts.MapFrom(x => x.FirstName))
                .ReverseMap()
                .ForMember(dest => dest.FirstName, opts => opts.MapFrom(x => x.FirstMidName))
            //.ForMember(dest => dest.FullName, opts => opts.MapFrom(x => $"{x.FirstMidName} {x.LastName}"));
            .ForMember(dest => dest.FullName, opts => opts.MapFrom(x => string.Concat(x.FirstMidName, " ", x.LastName)));

            CreateMissingTypeMaps = true;
        }
    }
}
