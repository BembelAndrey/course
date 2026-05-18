using Microsoft.EntityFrameworkCore;
using project.Models;
using System.Collections.Generic;

namespace project.Data
{
    public class AppDbContext : DbContext
    {
        public DbSet<User> Users { get; set; }
        public DbSet<Material> Materials { get; set; }
        public DbSet<Review> Reviews { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<OrderItem> OrderItems { get; set; }

        public AppDbContext()
        {
            string dbPath = System.IO.Path.Combine(System.AppDomain.CurrentDomain.BaseDirectory, "SoundProjectDb15.mdf");
            
            try
            {
                // Проверяем системную регистрацию базы в LocalDB
                using (var conn = new Microsoft.Data.SqlClient.SqlConnection(@"Server=(localdb)\mssqllocaldb;Database=master;Trusted_Connection=True;Connect Timeout=5"))
                {
                    conn.Open();
                    using (var cmd = conn.CreateCommand())
                    {
                        cmd.CommandText = "SELECT physical_name FROM sys.master_files WHERE name = 'SoundProjectDb15' AND database_id = DB_ID('SoundProjectDb15')";
                        var currentMappedPath = cmd.ExecuteScalar()?.ToString();

                        // Если путь в системе не совпадает с реальным расположением файла ИЛИ файла физически нет
                        if ((currentMappedPath != null && !string.Equals(currentMappedPath, dbPath, System.StringComparison.OrdinalIgnoreCase)) || !System.IO.File.Exists(dbPath))
                        {
                            cmd.CommandText = "IF EXISTS (SELECT name FROM sys.databases WHERE name = 'SoundProjectDb15') " +
                                              "BEGIN ALTER DATABASE [SoundProjectDb15] SET SINGLE_USER WITH ROLLBACK IMMEDIATE; " +
                                              "DROP DATABASE [SoundProjectDb15]; END";
                            cmd.ExecuteNonQuery();
                        }
                    }
                }
            }
            catch { }

            try
            {
                Database.EnsureCreated();
            }
            catch { }
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            string dbPath = System.IO.Path.Combine(System.AppDomain.CurrentDomain.BaseDirectory, "SoundProjectDb15.mdf");
            optionsBuilder.UseSqlServer($@"Server=(localdb)\mssqllocaldb;Database=SoundProjectDb15;AttachDbFilename={dbPath};Trusted_Connection=True;MultipleActiveResultSets=true;Connect Timeout=30");
            optionsBuilder.UseLazyLoadingProxies();
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Настройка связей: у одного пользователя может быть много заказов (как клиент)
            // и много выполненных заказов (как исполнитель)
            modelBuilder.Entity<Order>()
                .HasOne(o => o.User)
                .WithMany(u => u.Orders)
                .HasForeignKey(o => o.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Order>()
                .HasOne(o => o.Executor)
                .WithMany(u => u.ExecutedOrders)
                .HasForeignKey(o => o.ExecutorId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<User>().HasData(
                new User
                {
                    Id = 1,
                    Username = "admin",
                    PasswordHash = User.HashPassword("123456789"),
                    Email = "admin@soundproject.com",
                    FullNameRu = "Администратор Системы",
                    FullNameEn = "System Administrator",
                    Role = Role.Admin
                },
                new User
                {
                    Id = 2,
                    Username = "master",
                    PasswordHash = User.HashPassword("123456789"),
                    Email = "master@soundproject.com",
                    FullNameRu = "Иванов Иван Иванович",
                    FullNameEn = "Ivan Ivanovich Ivanov",
                    Role = Role.Executor
                },
                new User
                {
                    Id = 3,
                    Username = "master2",
                    PasswordHash = User.HashPassword("123456789"),
                    Email = "master2@soundproject.com",
                    FullNameRu = "Петров Петр Петрович",
                    FullNameEn = "Petr Petrovich Petrov",
                    Role = Role.Executor
                },
                new User
                {
                    Id = 4,
                    Username = "master3",
                    PasswordHash = User.HashPassword("123456789"),
                    Email = "master3@soundproject.com",
                    FullNameRu = "Сидоров Сидор Сидорович",
                    FullNameEn = "Sidor Sidorovich Sidorov",
                    Role = Role.Executor
                }
            );

            var initialMaterials = new List<Material>
            {
                new Material { Id = 1, CategoryRu = "Панели звукоизоляционные", CategoryEn = "CatSndPanels", TitleRu = "Панель звукоизоляционная", TitleEn = "Soundproofing panel", Price = 66.00m, DescriptionRu = "Звукоизоляционная панель SoundGuard ЭкоЗвукоИзол является инновационным материалом, не имеющим аналогов. Используется в качестве основного элемента шумозащитных конструкций и звукоизоляционных облицовок для жилья, офисов и др.", DescriptionEn = "The SoundGuard Eco-sound insulation panel is an innovative material that has no analogues. It is used as the main element of noise-proof structures and sound-proofing linings for housing, offices, etc.", ImagePath = @"Images\Products\ЭкоЗвукоИзол.jpg", StockQuantity = 100 },
                new Material { Id = 2, CategoryRu = "Панели звукоизоляционные", CategoryEn = "CatSndPanels", TitleRu = "Панель шумощит 18 для пола", TitleEn = "Noise shield 18 for floor", Price = 58.50m, DescriptionRu = "SoundGuard ШумоЩит – звукоизоляционная панель с пазогребневым соединением. Панель представляет собой многослойный гофрированный картонный профиль, заполненный мелкодисперсным песчаным наполнителем, с прикреплённым виброизоляционным слоем. Такой состав материала позволяет облегчить и ускорить монтаж звукоизоляционной конструкции.", DescriptionEn = "SoundGuard is a sound–proofing panel with a groove-ridge connection. The panel is a multi-layered corrugated cardboard profile filled with fine sand filler with an attached vibration-proofing layer. This material composition makes it easier and faster to install a sound insulation structure.", ImagePath = @"Images\Products\303.jpg", StockQuantity = 100 },
                new Material { Id = 3, CategoryRu = "Панели звукоизоляционные", CategoryEn = "CatSndPanels", TitleRu = "Панель звукоизоляционная за 88.50 $", TitleEn = "Premium soundproofing panel", Price = 88.50m, DescriptionRu = "Панели SoundGuard Premium являются инновационным материалом, панели используются как основной элемент шумозащитных облицовок жилья, офисов, на производстве и др.Панели обеспечивают многократное снижение энергии звука в широком диапазоне спектра. В отличие от строительных (пенобетон, кирпич, газосиликат) и отделочных (гипсокартон, пробка, линолеум) материалов, панели SG Premium снижают воздушный и ударный виды шума как в высокочастотном, так и в низкочастотном, басовом, диапазоне.", DescriptionEn = "SoundGuard Premium panels are an innovative material, panels are used as the main element of noise-proof cladding for housing, offices, manufacturing, etc.The panels provide multiple reductions in sound energy over a wide range of spectrum. Unlike construction (foam concrete, brick, gas silicate) and finishing (drywall, cork, linoleum) materials, SG Premium panels reduce airborne and impact noise in both the high-frequency and low-frequency, bass range.", ImagePath = @"Images\Products\Premium1.jpg", StockQuantity = 100 },
                new Material { Id = 4, CategoryRu = "Профили и листы", CategoryEn = "CatProfiles", TitleRu = "Лист ГВЛ", TitleEn = "GVL sheet", Price = 15.00m, DescriptionRu = "КНАУФ-суперлист влагостойкий (ГВЛВ) - однородный материал с высокой плотностью. Производится прессованием смеси гипсового вяжущего и волокон распушенной макулатуры. Применяется в звукоизоляционных, огнестойких и ударостойких конструкциях в зданиях и помещениях с сухим, нормальным и влажным влажностными режимами, в том числе в неотапливаемых помещениях. Обработан гидрофобизатором. Применение ГВЛВ позволяет исключить «мокрые» процессы и сократить сроки ремонтно-отделочных работ.", DescriptionEn = "KNAUF-super moisture-resistant sheet (GVLV) is a homogeneous material with high density. It is produced by pressing a mixture of gypsum binder and fluffed waste paper fibers. It is used in soundproof, fire-resistant and shock-resistant structures in buildings and rooms with dry, normal and humid humidity conditions, including in unheated rooms. Treated with a hydrophobizer. The use of hot water pumps makes it possible to eliminate \"wet\" processes and shorten the time of repair and finishing work.", ImagePath = @"Images\Products\120_original.jpg", StockQuantity = 500 },
                new Material { Id = 5, CategoryRu = "Профили и листы", CategoryEn = "CatProfiles", TitleRu = "Лист гипса knauf", TitleEn = "Gypsum sheet knauf", Price = 12.00m, DescriptionRu = "КНАУФ-лист влагоогнестойкий (ГСП-DFН2) сочетает в себе свойства огнестойкого и влагостойкого КНАУФ-листов.КНАУФ-лист влагоогнестойкий (ГСП-DFН2) применяется для устройства перегородок, подвесных потолков, облицовок стен, в зданиях и помещениях с сухим и нормальным влажностными режимами.", DescriptionEn = "Moisture-resistant KNAUF sheet (GSP-DFN2) combines the properties of fire-resistant and moisture-resistant KNAUF sheets.KNAUF moisture-resistant sheet (GSP-DFN2) is used for the installation of partitions, suspended ceilings, wall linings, in buildings and rooms with dry and normal humidity conditions.", ImagePath = @"Images\Products\Безназвания.jpg", StockQuantity = 500 },
                new Material { Id = 6, CategoryRu = "Профили и листы", CategoryEn = "CatProfiles", TitleRu = "Профиль CD knauf", TitleEn = "Profile CD knauf", Price = 4.00m, DescriptionRu = "Металлический профиль CD для каркасов.", DescriptionEn = "Metal CD profile for frames.", ImagePath = @"Images\Products\zxc.jpg", StockQuantity = 1000 },
                new Material { Id = 7, CategoryRu = "Профили и листы", CategoryEn = "CatProfiles", TitleRu = "Профиль ud knauf", TitleEn = "Profile UD knauf", Price = 3.00m, DescriptionRu = "Профиль UD – так называемый периметр. Крепится по всему периметру стен, т. е. везде, где гипсокартон примыкает к стенам. В него монтируется профиль CD. Профиль UD толщина металла 0.6 мм! Длина 3м.", DescriptionEn = "The UD profile is the so–called perimeter. It is attached along the entire perimeter of the walls, i.e. wherever the drywall is adjacent to the walls. The CD profile is mounted into it. UD profile metal thickness 0.6 mm! The length is 3m.", ImagePath = @"Images\Products\qwe.jpg", StockQuantity = 1000 },
                new Material { Id = 8, CategoryRu = "Эковата", CategoryEn = "CatEcowool", TitleRu = "Плита экоакустик 30", TitleEn = "EcoAcoustic 30 plate", Price = 78.00m, DescriptionRu = "Шумопоглощающая плита толщиной 50 мм для звукоизоляционных потолочных конструкций.Плиты звукопоглощающие SoundGuard ЭкоАкустик 30 закладываются в каркасные конструкции при звукоизоляции потолков. Главные их преимущества - стабильная упругость, оптимальная плотность, небольшой вес, высококачественное сырьё; плиты не содержат доменных отходов, фенол-формальдегидных смол.", DescriptionEn = "50 mm thick noise-absorbing plate for soundproof ceiling structures.SoundGuard Ecoacoustic 30 sound-absorbing plates are embedded in frame structures for sound insulation of ceilings. Their main advantages are stable elasticity, optimal density, low weight, high-quality raw materials; the plates do not contain blast furnace waste, phenol-formaldehyde resins.", ImagePath = @"Images\Products\f8a458e18503c86603dc02339a944feb_M.jpg", StockQuantity = 200 },
                new Material { Id = 9, CategoryRu = "Эковата", CategoryEn = "CatEcowool", TitleRu = "Плита ваты ЭкоАкустик 80", TitleEn = "EcoAcoustic 80 plate", Price = 240.00m, DescriptionRu = "SoundGuard ЭкоАкустик 80 - чистая минеральная вата, которая используется в звукоизоляционных облицовках пола. Имеет оптимальную плотность 75 кг/м3.Звукопоглощающая плита SG ЭкоАкустик 80 закладывается в шумозащитные конструкции для пола между лагами, используется при возведении стяжки. Главным преимуществом данной плиты, помимо оптимальных упругости и массы, является то, что она изготовлена из высококачественного сырья, не содержит доменных отходов и вредных примесей.", DescriptionEn = "SoundGuard Ecoacoustic 80 is pure mineral wool, which is used in sound insulation floor linings. It has an optimal density of 75 kg/m3.The SG Ecoacoustic 80 sound-absorbing plate is embedded in noise-proof structures for the floor between the logs, and is used in the construction of screeds. The main advantage of this plate, in addition to optimal elasticity and weight, is that it is made of high-quality raw materials, does not contain blast furnace waste and harmful impurities.", ImagePath = @"Images\Products\c75601cf4b798b9bb038a5b73c93d358_M.jpg", StockQuantity = 200 },
                new Material { Id = 10, CategoryRu = "Расходные материалы", CategoryEn = "CatConsumables", TitleRu = "Подложка демпферная", TitleEn = "Damper underlay", Price = 45.00m, DescriptionRu = "Подложка SoundGuard Roll предназначена для укладки под напольные чистовые покрытия, укладывается в качестве подложки под покрытия пола типа ламинат, паркетная доска, линолеум. Также активно используется в бескаркасном звукоизоляционном решении Эконом для стен в качестве первого слоя.", DescriptionEn = "The SoundGuard Roll substrate is designed for laying under floor finishing coatings, it is laid as a substrate under floor coverings such as laminate, parquet, linoleum. It is also actively used in frameless Economy sound insulation solution for walls as the first layer.", ImagePath = @"Images\Products\4b9f9da50cf2f358abdcd4a4321104f9_M.jpg", StockQuantity = 100 },
                new Material { Id = 11, CategoryRu = "Расходные материалы", CategoryEn = "CatConsumables", TitleRu = "Герметик для звукоизоляции seal 310", TitleEn = "Sealant seal 310", Price = 16.50m, DescriptionRu = "Высокоэластичный герметик, изготовленный на основе дисперсии полимеров акрила и силикона, используется при заделывании стыков, швов, трещин, строительных дефектов звукоизолирующих конструкций.", DescriptionEn = "A highly elastic sealant based on a dispersion of acrylic and silicone polymers is used for sealing joints, seams, cracks, and construction defects in soundproof structures.", ImagePath = @"Images\Products\fa55c8bad0e242eb7986dc1135b50adb_M.jpg", StockQuantity = 300 },
                new Material { Id = 12, CategoryRu = "Расходные материалы", CategoryEn = "CatConsumables", TitleRu = "Лента-скотч soundproject tape", TitleEn = "Tape soundproject tape", Price = 21.00m, DescriptionRu = "Специальная лента SG Tape предназначена для оклейки панелей SoundGuard при их обрезке, а также для проклейки стыков между панелями для недопущения попадания мелкого мусора. По структуре является силиконизированной бумажной лентой, обладающей усиленными клеящими свойствами.", DescriptionEn = "Special SG Tape is designed for gluing SoundGuard panels when they are being trimmed, as well as for gluing joints between panels to prevent small debris from entering. Structurally, it is a siliconized paper tape with enhanced adhesive properties.", ImagePath = @"Images\Products\ccb4e23c8aa216f1e96d31ab209c036b_M.jpg", StockQuantity = 300 },
                new Material { Id = 13, CategoryRu = "Расходные материалы", CategoryEn = "CatConsumables", TitleRu = "Саморезы 30x35", TitleEn = "Screws 30x35", Price = 0.50m, DescriptionRu = "Шуруп самонарезающий Knauf TN 3.5*35 с мелкой резьбой предназначен для крепления гипсокартонных листов (ГКЛ, ГКЛВ, ГКЛВО) к каркасу из металлических профилей (толщина стенки до 0,7 мм).", DescriptionEn = "The self-tapping screw Knauf TN 3.5*35 with fine thread is designed for fixing plasterboard sheets (GKL, GKLV, GKLVO) to a frame made of metal profiles (wall thickness up to 0.7 mm).", ImagePath = @"Images\Products\images.jpg", StockQuantity = 5000 },
                new Material { Id = 14, CategoryRu = "Расходные материалы", CategoryEn = "CatConsumables", TitleRu = "Саморезы 30x25", TitleEn = "Screws 30x25", Price = 0.40m, DescriptionRu = "Шуруп самонарезающий Knauf TN 3.5*25 с мелкой резьбой предназначен для крепления гипсокартонных листов (ГКЛ, ГКЛВ, ГКЛВО) к каркасу из металлических профилей (толщина стенки до 0,7 мм).", DescriptionEn = "The self-tapping screw Knauf TN 3.5*25 with fine thread is designed for fixing plasterboard sheets (GKL, GKLV, GKLVO) to a frame made of metal profiles (wall thickness up to 0.7 mm).", ImagePath = @"Images\Products\images(1).jpg", StockQuantity = 5000 },
                new Material { Id = 15, CategoryRu = "Расходные материалы", CategoryEn = "CatConsumables", TitleRu = "Дюбеля 3x25 knauf", TitleEn = "Dowels 3x25 knauf", Price = 0.60m, DescriptionRu = "Дюбель-гвоздь KNAUF PDG LK 3x25. Применяется для крепления элементов конструкций перегородок и облицовок (профилей, кронштейнов) к несущим основаниям (стены, перекрытия).", DescriptionEn = "Dowel-nail KNAUF PDG LK 3x25. It is used for fastening structural elements of partitions and linings (profiles, brackets) to load-bearing bases (walls, floors).", ImagePath = @"Images\Products\qweния.jpg", StockQuantity = 2000 },
                new Material { Id = 16, CategoryRu = "Крепления и подвесы", CategoryEn = "CatMounts", TitleRu = "Виброподвес protecktor", TitleEn = "Vibro-hanger protecktor", Price = 6.60m, DescriptionRu = "Виброподвес Vibro Protector обладает свойствами упругого демпфера и используется при устройстве звукоизолирующих потолков и стен, обеспечивая их структурную развязку со строительными конструкциями. Виброподвес Vibro Protector является подвесом с одной точкой крепления и сравнительно толстым слоем эластомера, он разработан специально для создания систем, имеющих максимальную изоляционную эффективность.", DescriptionEn = "The Vibro Protector suspension has the properties of an elastic damper and is used in the installation of soundproof ceilings and walls, ensuring their structural isolation from building structures. The Vibro Protector suspension is a suspension with a single attachment point and a relatively thick layer of elastomer, it is designed specifically to create systems with maximum insulating efficiency.", ImagePath = @"Images\Products\9ded0288e863fbe79d863f606cb05c21_M.jpg", StockQuantity = 500 },
                new Material { Id = 17, CategoryRu = "Крепления и подвесы", CategoryEn = "CatMounts", TitleRu = "Виброподвес vibro p", TitleEn = "Vibro-hanger vibro p", Price = 12.00m, DescriptionRu = "Виброподвес Vibro P обладает свойствами упругого демпфера и используется при устройстве звукоизолирующих потолков и стен, обеспечивая их структурную развязку со строительными конструкциями. Виброподвес Vibro P является подвесом с одной точкой крепления и сравнительно толстым слоем эластомера, он разработан специально для создания систем, имеющих максимальную изоляционную эффективность.", DescriptionEn = "The Vibro P vibration suspension has the properties of an elastic damper and is used in the installation of soundproof ceilings and walls, ensuring their structural isolation from building structures. The Vibro P suspension is a suspension with a single attachment point and a relatively thick layer of elastomer, it is designed specifically to create systems with maximum insulating efficiency.", ImagePath = @"Images\Products\e213534406f5e673030b12a49a117407_M.jpg", StockQuantity = 500 },
                new Material { Id = 18, CategoryRu = "Крепления и подвесы", CategoryEn = "CatMounts", TitleRu = "Виброподвес vibro pl", TitleEn = "Vibro-hanger vibro pl", Price = 18.00m, DescriptionRu = "Виброкрепление ВиброКреп pl используется при подвесном креплении к профильному листу или бетонному перекрытию. Благодаря своей форме, оно позволяет надежно крепить к профнастилу такие элементы, как вентиляционные короба, электролинии, кабельные трассы.", DescriptionEn = "Vibration fastening pl vibration fastener is used when suspended to a profile sheet or concrete floor. Due to its shape, it allows you to securely attach elements such as ventilation ducts, electrical lines, and cable runs to the corrugated board.", ImagePath = @"Images\Products\zszx(1).jpg", StockQuantity = 500 }
            };

            modelBuilder.Entity<Material>().HasData(initialMaterials);

            base.OnModelCreating(modelBuilder);
        }
    }
}