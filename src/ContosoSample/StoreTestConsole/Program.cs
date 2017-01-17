using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using ContosoUniversity.Contexts;
using ContosoUniversity.Crud.DataStores;
using ContosoUniversity.Data;
using ContosoUniversity.Data.Enitities;
using AutoMapper.QueryableExtensions;

namespace StoreTestConsole
{
    public class Program
    {
        public static void Main(string[] args)
        {
            IConfigurationRoot config = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                .Build();

            IServiceProvider serviceProvider = new ServiceCollection().AddDbContext<SchoolContext>(options =>
                options.UseSqlServer(config.GetConnectionString("DefaultConnection")), ServiceLifetime.Transient)
                .AddTransient<ISchoolStore, SchoolStore>()
                .BuildServiceProvider();

            Task.Run(async () =>
            {
                ISchoolStore store = serviceProvider.GetRequiredService<ISchoolStore>();

                int minNumber = await store.QueryAsync<Course>(q => q.Min(a => a.CourseID));
                List<Student> list = (await store.GetAsync<Student>(null,
                    null,
                    new Func<IQueryable<Student>, IIncludableQueryable<Student, object>>[] 
                    {
                        a => a.Include(x => x.Enrollments).ThenInclude(e => e.Course)
                    })).ToList();

                await Seed_Database(store);

            }).Wait();/**/

        }

        #region Seed DB
        private static async Task Seed_Database(ISchoolStore store)
        {
            if ((await store.CountAsync<Student>()) > 0)
                return;//database has been seeded

            Department[] departments = new Department[]
            {
                new Department
                {
                    EntityState = ContosoUniversity.Data.EntityStateType.Added,
                    Name = "English",     Budget = 350000,
                    StartDate = DateTime.Parse("2007-09-01"),
                    Administrator = new Instructor { FirstMidName = "Kim", LastName = "Abercrombie", HireDate = DateTime.Parse("1995-03-11")},
                    Courses =  new HashSet<Course>
                    {
                        new Course {CourseID = 2021, Title = "Composition",    Credits = 3},
                        new Course {CourseID = 2042, Title = "Literature",     Credits = 4}
                    }
                },
                new Department
                {
                    EntityState = ContosoUniversity.Data.EntityStateType.Added,
                    Name = "Mathematics",
                    Budget = 100000,
                    StartDate = DateTime.Parse("2007-09-01"),
                    Administrator = new Instructor
                    {
                        FirstMidName = "Fadi",
                        LastName = "Fakhouri",
                        HireDate = DateTime.Parse("2002-07-06"),
                        OfficeAssignment = new OfficeAssignment { Location = "Smith 17" }
                    },
                    Courses =  new HashSet<Course>
                    {
                        new Course {CourseID = 1045, Title = "Calculus",       Credits = 4},
                        new Course {CourseID = 3141, Title = "Trigonometry",   Credits = 4}
                    }
                },
                new Department
                {
                    EntityState = ContosoUniversity.Data.EntityStateType.Added,
                    Name = "Engineering", Budget = 350000,
                    StartDate = DateTime.Parse("2007-09-01"),
                    Administrator = new Instructor
                    {
                        FirstMidName = "Roger",
                        LastName = "Harui",
                        HireDate = DateTime.Parse("1998-07-01"),
                        OfficeAssignment = new OfficeAssignment { Location = "Gowan 27" }
                    },
                    Courses =  new HashSet<Course>
                    {
                        new Course {CourseID = 1050, Title = "Chemistry",      Credits = 3}
                    }
                },
                new Department
                {
                    EntityState = ContosoUniversity.Data.EntityStateType.Added,
                    Name = "Economics",
                    Budget = 100000,
                    StartDate = DateTime.Parse("2007-09-01"),
                    Administrator = new Instructor
                    {
                        FirstMidName = "Candace",
                        LastName = "Kapoor",
                        HireDate = DateTime.Parse("2001-01-15"),
                        OfficeAssignment = new OfficeAssignment { Location = "Thompson 304" }
                    },
                    Courses =  new HashSet<Course>
                    {
                        new Course {CourseID = 4022, Title = "Microeconomics", Credits = 3},
                        new Course {CourseID = 4041, Title = "Macroeconomics", Credits = 3 }
                    }
                }
            };
            await store.SaveGraphsAsync<Department>(departments);

            Instructor[] instructors = new Instructor[]
            {
                new Instructor
                {
                    FirstMidName = "Roger",   LastName = "Zheng",
                    HireDate = DateTime.Parse("2004-02-12"),
                    EntityState = ContosoUniversity.Data.EntityStateType.Added
                }
            };
            await store.SaveGraphsAsync<Instructor>(instructors);

            instructors = (await store.GetAsync<Instructor>()).ToArray();

            IEnumerable<Course> courses = departments.SelectMany(d => d.Courses);

            CourseAssignment[] courseInstructors = new CourseAssignment[]
            {
                new CourseAssignment {
                    EntityState = EntityStateType.Added,
                    CourseID = courses.Single(c => c.Title == "Chemistry" ).CourseID,
                    InstructorID = instructors.Single(i => i.LastName == "Kapoor").ID
                    },
                new CourseAssignment {
                    EntityState = EntityStateType.Added,
                    CourseID = courses.Single(c => c.Title == "Chemistry" ).CourseID,
                    InstructorID = instructors.Single(i => i.LastName == "Harui").ID
                    },
                new CourseAssignment {
                    EntityState = EntityStateType.Added,
                    CourseID = courses.Single(c => c.Title == "Microeconomics" ).CourseID,
                    InstructorID = instructors.Single(i => i.LastName == "Zheng").ID
                    },
                new CourseAssignment {
                    EntityState = EntityStateType.Added,
                    CourseID = courses.Single(c => c.Title == "Macroeconomics" ).CourseID,
                    InstructorID = instructors.Single(i => i.LastName == "Zheng").ID
                    },
                new CourseAssignment {
                    EntityState = EntityStateType.Added,
                    CourseID = courses.Single(c => c.Title == "Calculus" ).CourseID,
                    InstructorID = instructors.Single(i => i.LastName == "Fakhouri").ID
                    },
                new CourseAssignment {
                    EntityState = EntityStateType.Added,
                    CourseID = courses.Single(c => c.Title == "Trigonometry" ).CourseID,
                    InstructorID = instructors.Single(i => i.LastName == "Harui").ID
                    },
                new CourseAssignment {
                    EntityState = EntityStateType.Added,
                    CourseID = courses.Single(c => c.Title == "Composition" ).CourseID,
                    InstructorID = instructors.Single(i => i.LastName == "Abercrombie").ID
                    },
                new CourseAssignment {
                    EntityState = EntityStateType.Added,
                    CourseID = courses.Single(c => c.Title == "Literature" ).CourseID,
                    InstructorID = instructors.Single(i => i.LastName == "Abercrombie").ID
                    },
            };
            await store.SaveGraphsAsync<CourseAssignment>(courseInstructors);

            Student[] students = new Student[]
            {
                new Student
                {
                    EntityState = EntityStateType.Added,
                    FirstMidName = "Carson",   LastName = "Alexander",
                    EnrollmentDate = DateTime.Parse("2010-09-01"),
                    Enrollments = new HashSet<Enrollment>
                    {
                        new Enrollment
                        {
                            CourseID = courses.Single(c => c.Title == "Chemistry" ).CourseID,
                            Grade = Grade.A
                        },
                        new Enrollment
                        {
                            CourseID = courses.Single(c => c.Title == "Microeconomics" ).CourseID,
                            Grade = Grade.C
                        },
                        new Enrollment
                        {
                            CourseID = courses.Single(c => c.Title == "Macroeconomics" ).CourseID,
                            Grade = Grade.B
                        }
                    }
                },
                new Student
                {
                    EntityState = EntityStateType.Added,
                    FirstMidName = "Meredith", LastName = "Alonso",
                    EnrollmentDate = DateTime.Parse("2012-09-01"),
                    Enrollments = new HashSet<Enrollment>
                    {
                        new Enrollment
                        {
                            CourseID = courses.Single(c => c.Title == "Calculus" ).CourseID,
                            Grade = Grade.B
                        },
                        new Enrollment
                        {
                            CourseID = courses.Single(c => c.Title == "Trigonometry" ).CourseID,
                            Grade = Grade.B
                        },
                        new Enrollment
                        {
                            CourseID = courses.Single(c => c.Title == "Composition" ).CourseID,
                            Grade = Grade.B
                        }
                    }
                },
                new Student
                {
                    EntityState = EntityStateType.Added,
                    FirstMidName = "Arturo",   LastName = "Anand",
                    EnrollmentDate = DateTime.Parse("2013-09-01"),
                    Enrollments = new HashSet<Enrollment>
                    {
                        new Enrollment
                        {
                            CourseID = courses.Single(c => c.Title == "Chemistry" ).CourseID
                        },
                        new Enrollment
                        {
                            CourseID = courses.Single(c => c.Title == "Microeconomics").CourseID,
                            Grade = Grade.B
                        },
                    }
                },
                new Student
                {
                    EntityState = EntityStateType.Added,
                    FirstMidName = "Gytis",    LastName = "Barzdukas",
                    EnrollmentDate = DateTime.Parse("2012-09-01"),
                    Enrollments = new HashSet<Enrollment>
                    {
                        new Enrollment
                        {
                            CourseID = courses.Single(c => c.Title == "Chemistry").CourseID,
                            Grade = Grade.B
                        }
                    }
                },
                new Student
                {
                    EntityState = EntityStateType.Added,
                    FirstMidName = "Yan",      LastName = "Li",
                    EnrollmentDate = DateTime.Parse("2012-09-01"),
                    Enrollments = new HashSet<Enrollment>
                    {
                        new Enrollment
                        {
                            CourseID = courses.Single(c => c.Title == "Composition").CourseID,
                            Grade = Grade.B
                        }
                    }
                },
                new Student
                {
                    EntityState = EntityStateType.Added,
                    FirstMidName = "Peggy",    LastName = "Justice",
                    EnrollmentDate = DateTime.Parse("2011-09-01"),
                    Enrollments = new HashSet<Enrollment>
                    {
                        new Enrollment
                        {
                            CourseID = courses.Single(c => c.Title == "Literature").CourseID,
                            Grade = Grade.B
                        }
                    }
                },
                new Student
                {
                    EntityState = EntityStateType.Added,
                    FirstMidName = "Laura",    LastName = "Norman",
                    EnrollmentDate = DateTime.Parse("2013-09-01")
                },
                new Student
                {
                    EntityState = EntityStateType.Added,
                    FirstMidName = "Nino",     LastName = "Olivetto",
                    EnrollmentDate = DateTime.Parse("2005-09-01")
                }
            };
            await store.SaveGraphsAsync<Student>(students);
        }
        # endregion Seed DB
    }

    /*

        SchoolContext context = serviceProvider.GetRequiredService<SchoolContext>();
            StudentModel[] students = context.Students.Select(s => new StudentModel
            {
                ID = s.ID,
                FullName = s.FirstMidName + " " + s.LastName,
                EnrollmentDate = s.EnrollmentDate
            }).ToArray();

        StudentModel[] students2 = (await store.QueryAsync<Student>(q => q.Select(s => new StudentModel
                {
                    ID = s.ID,
                    FullName = s.FirstMidName + " " + s.LastName,
                    EnrollmentDate = s.EnrollmentDate
                }))).ToArray();






     #region ProjectTo
        public static async Task<ICollection<TModel>> GetAsync<TModel, TData>(Expression<Func<TModel, bool>> filter = null,
            Func<IQueryable<TModel>, IQueryable<TModel>> queryableFunc = null, ICollection<Func<IQueryable<TData>,
                IIncludableQueryable<TData, object>>> includeFuncs = null,
                SchoolContext context = null)
            where TData : class
        {

            IQueryable<TData> query = context.Set<TData>();

            if (includeFuncs != null)
                query = includeFuncs.Aggregate(query, (list, next) => query = next(query));

            if (filter != null)
                return queryableFunc != null
                    ? await queryableFunc(query.ProjectTo<TModel>()).Where(filter).ToListAsync()
                    : await query.ProjectTo<TModel>().Where(filter).ToListAsync();
            else
                return queryableFunc != null
                    ? await queryableFunc(query.ProjectTo<TModel>()).ToListAsync()
                    : await query.ProjectTo<TModel>().ToListAsync();
        }
        #endregion ProjectTo
         */
    public class StudentModel
    {
        public int ID { get; set; }
        public string LastName { get; set; }
        public string FirstName { get; set; }
        public string FullName { get; set; }
        public DateTime EnrollmentDate { get; set; }


    }
}
