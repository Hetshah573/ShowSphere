using Microsoft.EntityFrameworkCore;
using ShowSphere.Domain.Entities;
using ShowSphere.Domain.Enums;
using ShowSphere.Infrastructure.Data;

namespace ShowSphere.Infrastructure.Services;

public class DataSeeder
{
    private readonly ApplicationDbContext _context;

    public DataSeeder(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task SeedAsync()
    {
        if (await _context.Users.AnyAsync()) return;

        // Seed Genres
        var genres = new List<Genre>
        {
            new() { Id = 1, Name = "Action" },
            new() { Id = 2, Name = "Drama" },
            new() { Id = 3, Name = "Comedy" },
            new() { Id = 4, Name = "Thriller" },
            new() { Id = 5, Name = "Horror" },
            new() { Id = 6, Name = "Romance" },
            new() { Id = 7, Name = "Sci-Fi" },
            new() { Id = 8, Name = "Adventure" },
            new() { Id = 9, Name = "Animation" },
            new() { Id = 10, Name = "Mystery" },
            new() { Id = 11, Name = "Musical" },
            new() { Id = 12, Name = "Crime" },
            new() { Id = 13, Name = "Biography" },
            new() { Id = 14, Name = "Sports" },
            new() { Id = 15, Name = "Historical" }
        };
        _context.Genres.AddRange(genres);

        // Seed Admin User
        var admin = new User
        {
            Id = Guid.Parse("11111111-1111-1111-1111-111111111111"),
            Email = "hetshah11904@gmail.com",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("Admin@123"),
            FirstName = "Rajesh",
            LastName = "Sharma",
            Phone = "+919876543210",
            RoleId = 1,
            IsActive = true
        };

        // Seed Regular Users
        var user1 = new User
        {
            Id = Guid.Parse("22222222-2222-2222-2222-222222222222"),
            Email = "user@showsphere.com",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("User@123"),
            FirstName = "Priya",
            LastName = "Patel",
            Phone = "+919812345678",
            RoleId = 2,
            IsActive = true
        };

        var user2 = new User
        {
            Id = Guid.Parse("22222222-2222-2222-2222-222222222233"),
            Email = "amit.verma@gmail.com",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("User@123"),
            FirstName = "Amit",
            LastName = "Verma",
            Phone = "+919988776655",
            RoleId = 2,
            IsActive = true
        };

        var user3 = new User
        {
            Id = Guid.Parse("22222222-2222-2222-2222-222222222244"),
            Email = "sneha.reddy@gmail.com",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("User@123"),
            FirstName = "Sneha",
            LastName = "Reddy",
            Phone = "+919876501234",
            RoleId = 2,
            IsActive = true
        };

        _context.Users.AddRange(admin, user1, user2, user3);

        // Seed Cast - Indian Actors
        var cast = new List<Cast>
        {
            new() { Id = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa"), Name = "Shah Rukh Khan", PhotoUrl = "https://images.unsplash.com/photo-1506794778202-cad84cf45f1d?w=200&h=200&fit=crop" },
            new() { Id = Guid.Parse("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb"), Name = "Deepika Padukone", PhotoUrl = "https://images.unsplash.com/photo-1494790108377-be9c29b29330?w=200&h=200&fit=crop" },
            new() { Id = Guid.Parse("cccccccc-cccc-cccc-cccc-cccccccccccc"), Name = "Ranbir Kapoor", PhotoUrl = "https://images.unsplash.com/photo-1500648767791-00dcc994a43e?w=200&h=200&fit=crop" },
            new() { Id = Guid.Parse("dddddddd-dddd-dddd-dddd-dddddddddddd"), Name = "Alia Bhatt", PhotoUrl = "https://images.unsplash.com/photo-1534528741775-53994a69daeb?w=200&h=200&fit=crop" },
            new() { Id = Guid.Parse("eeeeeeee-eeee-eeee-eeee-eeeeeeeeeeee"), Name = "Hrithik Roshan", PhotoUrl = "https://images.unsplash.com/photo-1507003211169-0a1dd7228f2d?w=200&h=200&fit=crop" },
            new() { Id = Guid.Parse("ffffffff-ffff-ffff-ffff-ffffffffffff"), Name = "Prabhas", PhotoUrl = "https://images.unsplash.com/photo-1472099645785-5658abf4ff4e?w=200&h=200&fit=crop" },
            new() { Id = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaab"), Name = "Vijay Thalapathy", PhotoUrl = "https://images.unsplash.com/photo-1463453091185-61582044d556?w=200&h=200&fit=crop" },
            new() { Id = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaac"), Name = "Samantha Ruth Prabhu", PhotoUrl = "https://images.unsplash.com/photo-1544005313-94ddf0286df2?w=200&h=200&fit=crop" },
            new() { Id = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaad"), Name = "Ranveer Singh", PhotoUrl = "https://images.unsplash.com/photo-1531384441138-2736e62e0919?w=200&h=200&fit=crop" },
            new() { Id = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaae"), Name = "Rashmika Mandanna", PhotoUrl = "https://images.unsplash.com/photo-1517841905240-472988babdf9?w=200&h=200&fit=crop" },
            new() { Id = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaf"), Name = "Akshay Kumar", PhotoUrl = "https://images.unsplash.com/photo-1560250097-0b93528c311a?w=200&h=200&fit=crop" },
            new() { Id = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaa10"), Name = "Kiara Advani", PhotoUrl = "https://images.unsplash.com/photo-1488426862026-3ee34a7d66df?w=200&h=200&fit=crop" },
            new() { Id = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaa11"), Name = "Yash", PhotoUrl = "https://images.unsplash.com/photo-1492562080023-ab3db95bfbce?w=200&h=200&fit=crop" },
            new() { Id = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaa12"), Name = "Kartik Aaryan", PhotoUrl = "https://images.unsplash.com/photo-1519085360753-af0119f7cbe7?w=200&h=200&fit=crop" },
            new() { Id = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaa13"), Name = "Tamannaah Bhatia", PhotoUrl = "https://images.unsplash.com/photo-1524504388940-b1c1722653e1?w=200&h=200&fit=crop" },
            new() { Id = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaa14"), Name = "Rajkummar Rao", PhotoUrl = "https://images.unsplash.com/photo-1548372290-8d01b6c8e78c?w=200&h=200&fit=crop" },
            new() { Id = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaa15"), Name = "Shraddha Kapoor", PhotoUrl = "https://images.unsplash.com/photo-1502823403499-6ccfcf4fb453?w=200&h=200&fit=crop" },
            new() { Id = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaa16"), Name = "Allu Arjun", PhotoUrl = "https://images.unsplash.com/photo-1506794778202-cad84cf45f1d?w=200&h=200&fit=crop&crop=face" },
        };
        _context.Casts.AddRange(cast);

        // Seed Movies - Indian Cinema (Bollywood, Tollywood, Pan-India)
        var movies = new List<Movie>
        {
            // NOW SHOWING - Released movies
            new()
            {
                Id = Guid.Parse("33333333-3333-3333-3333-333333333333"),
                Title = "Pathaan 2",
                Description = "India's top spy Pathaan returns for an even bigger mission. When a global terror syndicate targets India's defence network, Pathaan must team up with unlikely allies across continents to stop an imminent attack that could change the world order forever.",
                PosterUrl = "https://images.unsplash.com/photo-1535016120720-40c646be5580?w=300&h=450&fit=crop",
                DurationMinutes = 162,
                Language = "Hindi",
                Certificate = MovieCertificate.UA,
                ReleaseDate = DateTime.UtcNow.AddDays(-5),
                AverageRating = 4.5m,
                TotalReviews = 2,
                IsActive = true
            },
            new()
            {
                Id = Guid.Parse("44444444-4444-4444-4444-444444444444"),
                Title = "Pushpa 3: The Rampage",
                Description = "Pushpa Raj now controls the red sandalwood empire, but enemies from every direction threaten his kingdom. As the police close in and rival smugglers unite against him, Pushpa must unleash his most dangerous side yet. The fire that started in the forests now engulfs the entire nation.",
                PosterUrl = "https://images.unsplash.com/photo-1574267432553-4b4628081c31?w=300&h=450&fit=crop",
                DurationMinutes = 178,
                Language = "Telugu",
                Certificate = MovieCertificate.UA,
                ReleaseDate = DateTime.UtcNow.AddDays(-10),
                AverageRating = 4.5m,
                TotalReviews = 2,
                IsActive = true
            },
            new()
            {
                Id = Guid.Parse("55555555-5555-5555-5555-555555555555"),
                Title = "Stree 3",
                Description = "The legends of Chanderi are far from over. A new supernatural entity has awakened — one that makes Stree look like a gentle ghost. Vicky and his friends must once again face their deepest fears while uncovering ancient secrets buried beneath their beloved town.",
                PosterUrl = "https://images.unsplash.com/photo-1635805737707-575885ab0820?w=300&h=450&fit=crop",
                DurationMinutes = 145,
                Language = "Hindi",
                Certificate = MovieCertificate.UA,
                ReleaseDate = DateTime.UtcNow.AddDays(-3),
                AverageRating = 4.0m,
                TotalReviews = 2,
                IsActive = true
            },
            new()
            {
                Id = Guid.Parse("66666666-6666-6666-6666-666666666666"),
                Title = "Singham Again",
                Description = "DCP Bajirao Singham faces his toughest challenge yet as a powerful crime lord with political connections threatens to tear apart Mumbai. With the entire system against him, Singham must choose between following orders and following his conscience.",
                PosterUrl = "https://images.unsplash.com/photo-1478720568477-152d9b164e26?w=300&h=450&fit=crop",
                DurationMinutes = 155,
                Language = "Hindi",
                Certificate = MovieCertificate.UA,
                ReleaseDate = DateTime.UtcNow.AddDays(-21),
                AverageRating = 0m,
                TotalReviews = 0,
                IsActive = true
            },
            new()
            {
                Id = Guid.Parse("77777777-7777-7777-7777-777777777777"),
                Title = "Devara: Part 2",
                Description = "The saga of Devara continues as the storm of the seas intensifies. Secrets of the past collide with battles of the present in this visually spectacular action drama set against the backdrop of coastal India's dangerous smuggling underworld.",
                PosterUrl = "https://images.unsplash.com/photo-1536440136628-849c177e76a1?w=300&h=450&fit=crop",
                DurationMinutes = 168,
                Language = "Telugu",
                Certificate = MovieCertificate.UA,
                ReleaseDate = DateTime.UtcNow.AddDays(-14),
                AverageRating = 0m,
                TotalReviews = 0,
                IsActive = true
            },
            new()
            {
                Id = Guid.Parse("88888888-8888-8888-8888-888888888888"),
                Title = "Chamkila",
                Description = "The untold story of Amar Singh Chamkila, Punjab's biggest music sensation of the 1980s. From singing in local gatherings to becoming the highest-selling artist of his era, witness the meteoric rise and controversial life of a man whose music divided a generation.",
                PosterUrl = "https://images.unsplash.com/photo-1511671782779-c97d3d27a1d4?w=300&h=450&fit=crop",
                DurationMinutes = 138,
                Language = "Punjabi",
                Certificate = MovieCertificate.A,
                ReleaseDate = DateTime.UtcNow.AddDays(-28),
                AverageRating = 5.0m,
                TotalReviews = 1,
                IsActive = true
            },
            new()
            {
                Id = Guid.Parse("99999999-9999-9999-9999-999999999999"),
                Title = "Bhool Bhulaiyaa 4",
                Description = "Rooh Baba is back! This time, a haunted palace in Rajasthan holds secrets that even he isn't prepared for. With multiple spirits, ancient curses, and comedic chaos, Rooh Baba must use all his wit and courage to survive the night.",
                PosterUrl = "https://images.unsplash.com/photo-1489599849927-2ee91cede3ba?w=300&h=450&fit=crop",
                DurationMinutes = 148,
                Language = "Hindi",
                Certificate = MovieCertificate.UA,
                ReleaseDate = DateTime.UtcNow.AddDays(-7),
                AverageRating = 4.0m,
                TotalReviews = 1,
                IsActive = true
            },
            new()
            {
                Id = Guid.Parse("11111111-1111-1111-1111-111111111112"),
                Title = "The Greatest of All Time",
                Description = "A retired RAW agent is pulled back into the world of espionage when his past catches up with him. Spanning two timelines, this high-octane thriller takes you from the streets of Chennai to the heart of a global conspiracy that only he can unravel.",
                PosterUrl = "https://images.unsplash.com/photo-1509347528160-9a9e33742cdb?w=300&h=450&fit=crop",
                DurationMinutes = 175,
                Language = "Tamil",
                Certificate = MovieCertificate.UA,
                ReleaseDate = DateTime.UtcNow.AddDays(-18),
                AverageRating = 0m,
                TotalReviews = 0,
                IsActive = true
            },
            new()
            {
                Id = Guid.Parse("11111111-1111-1111-1111-111111111113"),
                Title = "Laapataa Ladies",
                Description = "Two brides get accidentally swapped during a train journey in rural India. What follows is a heartwarming and hilarious journey of self-discovery, as both women navigate unexpected situations that challenge societal norms and reveal their true strength.",
                PosterUrl = "https://images.unsplash.com/photo-1502602898657-3e91760cbb34?w=300&h=450&fit=crop",
                DurationMinutes = 122,
                Language = "Hindi",
                Certificate = MovieCertificate.U,
                ReleaseDate = DateTime.UtcNow.AddDays(-25),
                AverageRating = 5.0m,
                TotalReviews = 1,
                IsActive = true
            },
            new()
            {
                Id = Guid.Parse("11111111-1111-1111-1111-111111111114"),
                Title = "Kalki 2899 AD: Ashwatthama",
                Description = "Set in a dystopian future, the chosen one rises to challenge the dark forces ruling Neo Kashi. With Ashwatthama as his guardian, an epic battle unfolds that will decide humanity's fate. Indian mythology meets futuristic sci-fi in this visual masterpiece.",
                PosterUrl = "https://images.unsplash.com/photo-1446776811953-b23d57bd21aa?w=300&h=450&fit=crop",
                DurationMinutes = 185,
                Language = "Telugu",
                Certificate = MovieCertificate.UA,
                ReleaseDate = DateTime.UtcNow.AddDays(-1),
                AverageRating = 4.67m,
                TotalReviews = 3,
                IsActive = true
            },

            // UPCOMING MOVIES
            new()
            {
                Id = Guid.Parse("11111111-1111-1111-1111-111111111115"),
                Title = "Dhoom 4",
                Description = "The most daring heist franchise in Indian cinema returns with a twist nobody saw coming. A mysterious thief with a personal vendetta against ACP Jai Dixit orchestrates robberies so elaborate they seem impossible — until you realize they're just the beginning.",
                PosterUrl = "https://images.unsplash.com/photo-1494976388531-d1058494cdd8?w=300&h=450&fit=crop",
                DurationMinutes = 158,
                Language = "Hindi",
                Certificate = MovieCertificate.UA,
                ReleaseDate = DateTime.UtcNow.AddDays(10),
                AverageRating = 0,
                TotalReviews = 0,
                IsActive = true
            },
            new()
            {
                Id = Guid.Parse("11111111-1111-1111-1111-111111111116"),
                Title = "RRR 2: Rise of Bheem",
                Description = "The legendary freedom fighter Komaram Bheem's story continues. After the events with Ram, Bheem returns to his tribal land only to discover a new threat — one that requires him to unite all tribes of the Deccan against the might of a corrupt empire.",
                PosterUrl = "https://images.unsplash.com/photo-1533488765986-dfa2a9939acd?w=300&h=450&fit=crop",
                DurationMinutes = 190,
                Language = "Telugu",
                Certificate = MovieCertificate.UA,
                ReleaseDate = DateTime.UtcNow.AddDays(21),
                AverageRating = 0,
                TotalReviews = 0,
                IsActive = true
            },
            new()
            {
                Id = Guid.Parse("11111111-1111-1111-1111-111111111117"),
                Title = "Hera Pheri 3",
                Description = "Raju, Shyam, and Baburao are back in the craziest adventure yet! When a get-rich-quick scheme involving cryptocurrency goes hilariously wrong, the iconic trio must dodge gangsters, police, and each other in this laugh riot that fans have waited two decades for.",
                PosterUrl = "https://images.unsplash.com/photo-1485846234645-a62644f84728?w=300&h=450&fit=crop",
                DurationMinutes = 142,
                Language = "Hindi",
                Certificate = MovieCertificate.U,
                ReleaseDate = DateTime.UtcNow.AddDays(30),
                AverageRating = 0,
                TotalReviews = 0,
                IsActive = true
            },
            new()
            {
                Id = Guid.Parse("11111111-1111-1111-1111-111111111118"),
                Title = "Salaar 2: Shouryanga Parvam",
                Description = "Deva's story reaches its devastating climax. Betrayals run deep as the battle for power in the lawless lands reaches a point of no return. Friendship and duty clash in this dark, intense saga of loyalty tested to its absolute limits.",
                PosterUrl = "https://images.unsplash.com/photo-1440404653325-ab127d49abc1?w=300&h=450&fit=crop",
                DurationMinutes = 172,
                Language = "Telugu",
                Certificate = MovieCertificate.UA,
                ReleaseDate = DateTime.UtcNow.AddDays(14),
                AverageRating = 0,
                TotalReviews = 0,
                IsActive = true
            },
            new()
            {
                Id = Guid.Parse("11111111-1111-1111-1111-111111111119"),
                Title = "Welcome 4",
                Description = "The crazy world of Uday Bhai and Majnu Bhai expands as they accidentally become international diplomats. With a peace summit at stake and their antics causing one disaster after another, hilarity reaches new heights in this ensemble comedy.",
                PosterUrl = "https://images.unsplash.com/photo-1517604931442-7e0c8ed2963c?w=300&h=450&fit=crop",
                DurationMinutes = 150,
                Language = "Hindi",
                Certificate = MovieCertificate.U,
                ReleaseDate = DateTime.UtcNow.AddDays(45),
                AverageRating = 0,
                TotalReviews = 0,
                IsActive = true
            },
        };
        _context.Movies.AddRange(movies);

        // Seed Movie Genres
        _context.MovieGenres.AddRange(
            // Pathaan 2 - Action, Thriller
            new MovieGenre { MovieId = movies[0].Id, GenreId = 1 },
            new MovieGenre { MovieId = movies[0].Id, GenreId = 4 },
            // Pushpa 3 - Action, Drama, Crime
            new MovieGenre { MovieId = movies[1].Id, GenreId = 1 },
            new MovieGenre { MovieId = movies[1].Id, GenreId = 2 },
            new MovieGenre { MovieId = movies[1].Id, GenreId = 12 },
            // Stree 3 - Horror, Comedy
            new MovieGenre { MovieId = movies[2].Id, GenreId = 5 },
            new MovieGenre { MovieId = movies[2].Id, GenreId = 3 },
            // Singham Again - Action, Drama
            new MovieGenre { MovieId = movies[3].Id, GenreId = 1 },
            new MovieGenre { MovieId = movies[3].Id, GenreId = 2 },
            // Devara Part 2 - Action, Thriller
            new MovieGenre { MovieId = movies[4].Id, GenreId = 1 },
            new MovieGenre { MovieId = movies[4].Id, GenreId = 4 },
            // Chamkila - Drama, Biography, Musical
            new MovieGenre { MovieId = movies[5].Id, GenreId = 2 },
            new MovieGenre { MovieId = movies[5].Id, GenreId = 13 },
            new MovieGenre { MovieId = movies[5].Id, GenreId = 11 },
            // Bhool Bhulaiyaa 4 - Horror, Comedy
            new MovieGenre { MovieId = movies[6].Id, GenreId = 5 },
            new MovieGenre { MovieId = movies[6].Id, GenreId = 3 },
            // GOAT - Action, Thriller
            new MovieGenre { MovieId = movies[7].Id, GenreId = 1 },
            new MovieGenre { MovieId = movies[7].Id, GenreId = 4 },
            // Laapataa Ladies - Comedy, Drama
            new MovieGenre { MovieId = movies[8].Id, GenreId = 3 },
            new MovieGenre { MovieId = movies[8].Id, GenreId = 2 },
            // Kalki 2899 AD - Sci-Fi, Action, Adventure
            new MovieGenre { MovieId = movies[9].Id, GenreId = 7 },
            new MovieGenre { MovieId = movies[9].Id, GenreId = 1 },
            new MovieGenre { MovieId = movies[9].Id, GenreId = 8 },
            // Dhoom 4 - Action, Thriller
            new MovieGenre { MovieId = movies[10].Id, GenreId = 1 },
            new MovieGenre { MovieId = movies[10].Id, GenreId = 4 },
            // RRR 2 - Action, Drama, Historical
            new MovieGenre { MovieId = movies[11].Id, GenreId = 1 },
            new MovieGenre { MovieId = movies[11].Id, GenreId = 2 },
            new MovieGenre { MovieId = movies[11].Id, GenreId = 15 },
            // Hera Pheri 3 - Comedy
            new MovieGenre { MovieId = movies[12].Id, GenreId = 3 },
            // Salaar 2 - Action, Thriller
            new MovieGenre { MovieId = movies[13].Id, GenreId = 1 },
            new MovieGenre { MovieId = movies[13].Id, GenreId = 4 },
            // Welcome 4 - Comedy
            new MovieGenre { MovieId = movies[14].Id, GenreId = 3 }
        );

        // Seed Movie Cast
        _context.MovieCasts.AddRange(
            // Pathaan 2
            new MovieCast { MovieId = movies[0].Id, CastId = cast[0].Id, Role = "Pathaan" },
            new MovieCast { MovieId = movies[0].Id, CastId = cast[1].Id, Role = "Rubina" },
            new MovieCast { MovieId = movies[0].Id, CastId = cast[4].Id, Role = "Jim" },
            // Pushpa 3
            new MovieCast { MovieId = movies[1].Id, CastId = cast[17].Id, Role = "Pushpa Raj" },
            new MovieCast { MovieId = movies[1].Id, CastId = cast[9].Id, Role = "Srivalli" },
            // Stree 3
            new MovieCast { MovieId = movies[2].Id, CastId = cast[15].Id, Role = "Vicky" },
            new MovieCast { MovieId = movies[2].Id, CastId = cast[16].Id, Role = "Stree" },
            // Singham Again
            new MovieCast { MovieId = movies[3].Id, CastId = cast[10].Id, Role = "DCP Bajirao Singham" },
            new MovieCast { MovieId = movies[3].Id, CastId = cast[11].Id, Role = "Avni" },
            new MovieCast { MovieId = movies[3].Id, CastId = cast[8].Id, Role = "Simmba" },
            // Devara Part 2
            new MovieCast { MovieId = movies[4].Id, CastId = cast[5].Id, Role = "Devara" },
            // Chamkila
            new MovieCast { MovieId = movies[5].Id, CastId = cast[8].Id, Role = "Amar Singh Chamkila" },
            new MovieCast { MovieId = movies[5].Id, CastId = cast[3].Id, Role = "Amarjot Kaur" },
            // Bhool Bhulaiyaa 4
            new MovieCast { MovieId = movies[6].Id, CastId = cast[13].Id, Role = "Rooh Baba" },
            new MovieCast { MovieId = movies[6].Id, CastId = cast[14].Id, Role = "Manjulika" },
            // GOAT
            new MovieCast { MovieId = movies[7].Id, CastId = cast[6].Id, Role = "Gandhi" },
            // Laapataa Ladies
            new MovieCast { MovieId = movies[8].Id, CastId = cast[3].Id, Role = "Narrator" },
            // Kalki 2899 AD
            new MovieCast { MovieId = movies[9].Id, CastId = cast[5].Id, Role = "Bhairava" },
            new MovieCast { MovieId = movies[9].Id, CastId = cast[1].Id, Role = "Sumathi" },
            new MovieCast { MovieId = movies[9].Id, CastId = cast[2].Id, Role = "Ashwatthama" },
            // Dhoom 4
            new MovieCast { MovieId = movies[10].Id, CastId = cast[4].Id, Role = "ACP Jai Dixit" },
            new MovieCast { MovieId = movies[10].Id, CastId = cast[2].Id, Role = "Aryan" },
            // RRR 2
            new MovieCast { MovieId = movies[11].Id, CastId = cast[5].Id, Role = "Komaram Bheem" },
            // Hera Pheri 3
            new MovieCast { MovieId = movies[12].Id, CastId = cast[10].Id, Role = "Raju" },
            // Salaar 2
            new MovieCast { MovieId = movies[13].Id, CastId = cast[5].Id, Role = "Deva" },
            new MovieCast { MovieId = movies[13].Id, CastId = cast[16].Id, Role = "Aadhya" },
            // Welcome 4
            new MovieCast { MovieId = movies[14].Id, CastId = cast[10].Id, Role = "Uday Bhai" }
        );

        // Seed Theaters - Realistic Indian Multiplex Chains
        var theater1 = new Theater
        {
            Id = Guid.Parse("aaa11111-1111-1111-1111-111111111111"),
            Name = "PVR INOX - Phoenix Palladium",
            Address = "462, Senapati Bapat Marg, Lower Parel",
            City = "Mumbai",
            State = "Maharashtra",
            PinCode = "400013",
            IsActive = true
        };

        var theater2 = new Theater
        {
            Id = Guid.Parse("bbb22222-2222-2222-2222-222222222222"),
            Name = "INOX Megaplex - Inorbit Mall",
            Address = "Inorbit Mall, Link Road, Malad West",
            City = "Mumbai",
            State = "Maharashtra",
            PinCode = "400064",
            IsActive = true
        };

        var theater3 = new Theater
        {
            Id = Guid.Parse("ccc33333-3333-3333-3333-333333333333"),
            Name = "PVR Director's Cut - Ambience Mall",
            Address = "Ambience Mall, Nelson Mandela Road, Vasant Kunj",
            City = "New Delhi",
            State = "Delhi",
            PinCode = "110070",
            IsActive = true
        };

        var theater4 = new Theater
        {
            Id = Guid.Parse("aaa11111-1111-1111-1111-111111111122"),
            Name = "Cinepolis - Forum Mall",
            Address = "Forum Value Mall, Whitefield Main Road",
            City = "Bengaluru",
            State = "Karnataka",
            PinCode = "560066",
            IsActive = true
        };

        var theater5 = new Theater
        {
            Id = Guid.Parse("aaa11111-1111-1111-1111-111111111133"),
            Name = "PVR IMAX - Express Avenue",
            Address = "Express Avenue Mall, Whites Road, Royapettah",
            City = "Chennai",
            State = "Tamil Nadu",
            PinCode = "600014",
            IsActive = true
        };

        var theater6 = new Theater
        {
            Id = Guid.Parse("aaa11111-1111-1111-1111-111111111144"),
            Name = "AMB Cinemas",
            Address = "Gachibowli, Hyderabad",
            City = "Hyderabad",
            State = "Telangana",
            PinCode = "500032",
            IsActive = true
        };

        _context.Theaters.AddRange(theater1, theater2, theater3, theater4, theater5, theater6);

        // Seed Screens
        var screen1 = new Screen
        {
            Id = Guid.Parse("ddd11111-1111-1111-1111-111111111111"),
            TheaterId = theater1.Id,
            Name = "Audi 1",
            TotalSeats = 220,
            ScreenType = ScreenType.IMAX
        };

        var screen2 = new Screen
        {
            Id = Guid.Parse("ddd22222-2222-2222-2222-222222222222"),
            TheaterId = theater1.Id,
            Name = "Audi 2",
            TotalSeats = 150,
            ScreenType = ScreenType.Dolby
        };

        var screen3 = new Screen
        {
            Id = Guid.Parse("ddd33333-3333-3333-3333-333333333333"),
            TheaterId = theater1.Id,
            Name = "Audi 3",
            TotalSeats = 120,
            ScreenType = ScreenType.Standard
        };

        var screen4 = new Screen
        {
            Id = Guid.Parse("ddd44444-4444-4444-4444-444444444444"),
            TheaterId = theater2.Id,
            Name = "Screen 1 - IMAX",
            TotalSeats = 280,
            ScreenType = ScreenType.IMAX
        };

        var screen5 = new Screen
        {
            Id = Guid.Parse("ddd55555-5555-5555-5555-555555555555"),
            TheaterId = theater2.Id,
            Name = "Screen 2 - 4DX",
            TotalSeats = 100,
            ScreenType = ScreenType.FourDX
        };

        var screen6 = new Screen
        {
            Id = Guid.Parse("ddd66666-6666-6666-6666-666666666666"),
            TheaterId = theater3.Id,
            Name = "Director's Cut Lounge",
            TotalSeats = 50,
            ScreenType = ScreenType.Dolby
        };

        var screen7 = new Screen
        {
            Id = Guid.Parse("ddd77777-7777-7777-7777-777777777777"),
            TheaterId = theater3.Id,
            Name = "Audi 1",
            TotalSeats = 180,
            ScreenType = ScreenType.Standard
        };

        var screen8 = new Screen
        {
            Id = Guid.Parse("ddd88888-8888-8888-8888-888888888888"),
            TheaterId = theater4.Id,
            Name = "Screen 1",
            TotalSeats = 200,
            ScreenType = ScreenType.Standard
        };

        var screen9 = new Screen
        {
            Id = Guid.Parse("ddd99999-9999-9999-9999-999999999999"),
            TheaterId = theater4.Id,
            Name = "Screen 2 - Macro XE",
            TotalSeats = 160,
            ScreenType = ScreenType.IMAX
        };

        var screen10 = new Screen
        {
            Id = Guid.Parse("dddaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa"),
            TheaterId = theater5.Id,
            Name = "IMAX Screen",
            TotalSeats = 300,
            ScreenType = ScreenType.IMAX
        };

        var screen11 = new Screen
        {
            Id = Guid.Parse("dddaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaab"),
            TheaterId = theater5.Id,
            Name = "Screen 2",
            TotalSeats = 180,
            ScreenType = ScreenType.Standard
        };

        var screen12 = new Screen
        {
            Id = Guid.Parse("dddaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaac"),
            TheaterId = theater6.Id,
            Name = "Atmos Screen",
            TotalSeats = 240,
            ScreenType = ScreenType.Dolby
        };

        var screens = new[] { screen1, screen2, screen3, screen4, screen5, screen6, screen7, screen8, screen9, screen10, screen11, screen12 };
        _context.Screens.AddRange(screens);

        // Seed Seats for each screen
        foreach (var screen in screens)
        {
            SeedSeatsForScreen(screen.Id, screen.TotalSeats, screen.ScreenType);
        }

        // Seed Shows - Multiple shows per day for next 7 days
        var screenMovieAssignments = new (Screen screen, Movie movie, decimal basePrice)[]
        {
            // PVR Phoenix - Mumbai
            (screen1, movies[9], 550m),  // Kalki 2899 AD in IMAX
            (screen1, movies[1], 550m),  // Pushpa 3 in IMAX
            (screen2, movies[0], 400m),  // Pathaan 2 in Dolby
            (screen2, movies[3], 400m),  // Singham Again in Dolby
            (screen3, movies[2], 280m),  // Stree 3 Standard
            (screen3, movies[6], 280m),  // Bhool Bhulaiyaa 4 Standard

            // INOX Megaplex - Mumbai
            (screen4, movies[9], 600m),  // Kalki IMAX
            (screen4, movies[4], 600m),  // Devara Part 2 IMAX
            (screen5, movies[0], 700m),  // Pathaan 2 in 4DX
            (screen5, movies[2], 700m),  // Stree 3 in 4DX

            // PVR Director's Cut - Delhi
            (screen6, movies[5], 1200m), // Chamkila Director's Cut
            (screen6, movies[8], 1200m), // Laapataa Ladies Director's Cut
            (screen7, movies[0], 320m),  // Pathaan 2
            (screen7, movies[7], 320m),  // GOAT

            // Cinepolis - Bengaluru
            (screen8, movies[1], 250m),  // Pushpa 3
            (screen8, movies[6], 250m),  // Bhool Bhulaiyaa 4
            (screen9, movies[9], 480m),  // Kalki IMAX
            (screen9, movies[3], 480m),  // Singham Again IMAX

            // PVR IMAX - Chennai
            (screen10, movies[7], 500m), // GOAT IMAX
            (screen10, movies[9], 500m), // Kalki IMAX
            (screen11, movies[4], 300m), // Devara Part 2
            (screen11, movies[1], 300m), // Pushpa 3

            // AMB - Hyderabad
            (screen12, movies[9], 450m), // Kalki Dolby
            (screen12, movies[1], 450m), // Pushpa 3 Dolby
        };

        var showTimesSlots = new[] { 9.0, 12.5, 15.5, 18.5, 21.5 }; // 9AM, 12:30PM, 3:30PM, 6:30PM, 9:30PM

        foreach (var (screen, movie, basePrice) in screenMovieAssignments)
        {
            if (movie.ReleaseDate > DateTime.UtcNow.AddDays(1)) continue;

            for (var day = 0; day < 7; day++)
            {
                foreach (var hour in showTimesSlots.Take(day == 0 ? 4 : 5))
                {
                    var startTime = DateTime.UtcNow.Date.AddDays(day).AddHours(hour);
                    if (startTime < DateTime.UtcNow) continue;

                    _context.Shows.Add(new Show
                    {
                        MovieId = movie.Id,
                        ScreenId = screen.Id,
                        StartTime = startTime,
                        EndTime = startTime.AddMinutes(movie.DurationMinutes + 20),
                        BasePrice = basePrice,
                        IsActive = true
                    });
                }
            }
        }

        // Seed some sample bookings for dashboard stats
        var bookings = new List<Booking>
        {
            new()
            {
                Id = Guid.NewGuid(),
                UserId = user1.Id,
                ShowId = Guid.Empty, // Will be set after shows are saved
                BookingNumber = "SS-20260520-001",
                TotalSeats = 2,
                TotalAmount = 1100m,
                Status = BookingStatus.Confirmed,
                QRCode = "SHOWSPHERE|SS-20260520-001|dce30ef23556c7c72f647e4ee10d1fc3730f5ee5ae7057c2a216d03882af1af0",
                CreatedAt = DateTime.UtcNow.AddDays(-2)
            },
            new()
            {
                Id = Guid.NewGuid(),
                UserId = user2.Id,
                ShowId = Guid.Empty,
                BookingNumber = "SS-20260521-002",
                TotalSeats = 2,
                TotalAmount = 800m,
                Status = BookingStatus.Confirmed,
                QRCode = "SHOWSPHERE|SS-20260521-002|2831025df6baa15eb286f2d23c7ff20895bf1a2e691fb48116c2a12bfe7086b7",
                CreatedAt = DateTime.UtcNow.AddDays(-1)
            },
            new()
            {
                Id = Guid.NewGuid(),
                UserId = user3.Id,
                ShowId = Guid.Empty,
                BookingNumber = "SS-20260522-003",
                TotalSeats = 4,
                TotalAmount = 2400m,
                Status = BookingStatus.Confirmed,
                QRCode = "SHOWSPHERE|SS-20260522-003|ddae0f2055b5f07266cec61a63c653359ca3a5bea62d6eb6888f8e4c5e746a83",
                CreatedAt = DateTime.UtcNow.AddHours(-6)
            },
            new()
            {
                Id = Guid.NewGuid(),
                UserId = user1.Id,
                ShowId = Guid.Empty,
                BookingNumber = "SS-20260522-004",
                TotalSeats = 2,
                TotalAmount = 560m,
                Status = BookingStatus.Pending,
                QRCode = "SHOWSPHERE|SS-20260522-004|7fa789532c7209ce00133b234094f16608f598171004ae29c8a1e994e9a39dba",
                CreatedAt = DateTime.UtcNow.AddHours(-2)
            },
            new()
            {
                Id = Guid.NewGuid(),
                UserId = user2.Id,
                ShowId = Guid.Empty,
                BookingNumber = "SS-20260519-005",
                TotalSeats = 2,
                TotalAmount = 1400m,
                Status = BookingStatus.Cancelled,
                QRCode = "SHOWSPHERE|SS-20260519-005|8ee40e4fa4f9f47dd7c0ffbd8b606d0ae2d168d600925159abbd406f979aac2f",
                CreatedAt = DateTime.UtcNow.AddDays(-3)
            },
        };

        // Seed Reviews
        var reviews = new List<Review>
        {
            new() { Id = Guid.NewGuid(), MovieId = movies[0].Id, UserId = user1.Id, Rating = 4, Comment = "Mass entertainer! SRK is on fire 🔥. Interval block was insane.", CreatedAt = DateTime.UtcNow.AddDays(-3) },
            new() { Id = Guid.NewGuid(), MovieId = movies[0].Id, UserId = user2.Id, Rating = 5, Comment = "Pathaan is back and how! Action sequences are world-class.", CreatedAt = DateTime.UtcNow.AddDays(-4) },
            new() { Id = Guid.NewGuid(), MovieId = movies[1].Id, UserId = user1.Id, Rating = 5, Comment = "Allu Arjun sir ki acting zabardast hai! Pushpa never disappoints.", CreatedAt = DateTime.UtcNow.AddDays(-8) },
            new() { Id = Guid.NewGuid(), MovieId = movies[1].Id, UserId = user3.Id, Rating = 4, Comment = "Blockbuster! Climax fight was too good. Telugu cinema rocks!", CreatedAt = DateTime.UtcNow.AddDays(-7) },
            new() { Id = Guid.NewGuid(), MovieId = movies[2].Id, UserId = user2.Id, Rating = 4, Comment = "Rajkummar Rao is hilarious as always. Comedy + horror done right!", CreatedAt = DateTime.UtcNow.AddDays(-2) },
            new() { Id = Guid.NewGuid(), MovieId = movies[2].Id, UserId = user3.Id, Rating = 4, Comment = "Shraddha's track is the highlight. Great fun with friends and family!", CreatedAt = DateTime.UtcNow.AddDays(-1) },
            new() { Id = Guid.NewGuid(), MovieId = movies[5].Id, UserId = user1.Id, Rating = 5, Comment = "Diljit as Chamkila is perfect casting. What a performance!", CreatedAt = DateTime.UtcNow.AddDays(-20) },
            new() { Id = Guid.NewGuid(), MovieId = movies[8].Id, UserId = user2.Id, Rating = 5, Comment = "Such a beautiful film! Simple story told brilliantly. Kiran Rao is a genius.", CreatedAt = DateTime.UtcNow.AddDays(-18) },
            new() { Id = Guid.NewGuid(), MovieId = movies[9].Id, UserId = user1.Id, Rating = 5, Comment = "Indian cinema ka future yahi hai! Visuals are out of this world. Prabhas is amazing!", CreatedAt = DateTime.UtcNow.AddHours(-12) },
            new() { Id = Guid.NewGuid(), MovieId = movies[9].Id, UserId = user2.Id, Rating = 5, Comment = "Goosebumps throughout! Nag Ashwin sir vision is unmatched. Deepika 😍", CreatedAt = DateTime.UtcNow.AddHours(-8) },
            new() { Id = Guid.NewGuid(), MovieId = movies[9].Id, UserId = user3.Id, Rating = 4, Comment = "Visual masterpiece. Second half is slightly slow but climax makes up for it.", CreatedAt = DateTime.UtcNow.AddHours(-5) },
            new() { Id = Guid.NewGuid(), MovieId = movies[6].Id, UserId = user3.Id, Rating = 4, Comment = "Kartik Aaryan has found his niche! Funny and spooky. Paisa vasool!", CreatedAt = DateTime.UtcNow.AddDays(-5) },
        };
        _context.Reviews.AddRange(reviews);

        await _context.SaveChangesAsync();

        // Now set ShowId for bookings (grab first available shows)
        var savedShows = await _context.Shows.Take(5).ToListAsync();
        if (savedShows.Count >= 5)
        {
            for (int i = 0; i < bookings.Count && i < savedShows.Count; i++)
            {
                bookings[i].ShowId = savedShows[i].Id;
            }
            _context.Bookings.AddRange(bookings);
            await _context.SaveChangesAsync();

            // Seed Payments (after bookings are saved)
            var payments = new List<Payment>
            {
                new() { Id = Guid.NewGuid(), BookingId = bookings[0].Id, Amount = 1100m, Method = PaymentMethod.UPI, Status = PaymentStatus.Completed, TransactionId = "UPI-" + Guid.NewGuid().ToString()[..8].ToUpper(), CreatedAt = DateTime.UtcNow.AddDays(-2) },
                new() { Id = Guid.NewGuid(), BookingId = bookings[1].Id, Amount = 800m, Method = PaymentMethod.DebitCard, Status = PaymentStatus.Completed, TransactionId = "DC-" + Guid.NewGuid().ToString()[..8].ToUpper(), CreatedAt = DateTime.UtcNow.AddDays(-1) },
                new() { Id = Guid.NewGuid(), BookingId = bookings[2].Id, Amount = 2400m, Method = PaymentMethod.CreditCard, Status = PaymentStatus.Completed, TransactionId = "CC-" + Guid.NewGuid().ToString()[..8].ToUpper(), CreatedAt = DateTime.UtcNow.AddHours(-6) },
                new() { Id = Guid.NewGuid(), BookingId = bookings[3].Id, Amount = 560m, Method = PaymentMethod.UPI, Status = PaymentStatus.Pending, TransactionId = "UPI-" + Guid.NewGuid().ToString()[..8].ToUpper(), CreatedAt = DateTime.UtcNow.AddHours(-2) },
                new() { Id = Guid.NewGuid(), BookingId = bookings[4].Id, Amount = 1400m, Method = PaymentMethod.Wallet, Status = PaymentStatus.Refunded, TransactionId = "WAL-" + Guid.NewGuid().ToString()[..8].ToUpper(), CreatedAt = DateTime.UtcNow.AddDays(-3) },
            };
            _context.Payments.AddRange(payments);
            await _context.SaveChangesAsync();

            // Seed BookingSeats — pick real seat IDs from each booking's show's screen
            var bookingSeats = new List<BookingSeat>();
            foreach (var booking in bookings)
            {
                if (booking.ShowId == Guid.Empty) continue;
                var show = await _context.Shows
                    .FirstOrDefaultAsync(s => s.Id == booking.ShowId);
                if (show == null) continue;
                var seats = await _context.Seats
                    .Where(s => s.ScreenId == show.ScreenId && s.IsActive)
                    .OrderBy(s => s.Row).ThenBy(s => s.Number)
                    .Take(booking.TotalSeats)
                    .ToListAsync();
                foreach (var seat in seats)
                {
                    bookingSeats.Add(new BookingSeat
                    {
                        Id = Guid.NewGuid(),
                        BookingId = booking.Id,
                        SeatId = seat.Id,
                        Price = seat.Price,
                        Status = booking.Status
                    });
                }
            }
            if (bookingSeats.Count > 0)
            {
                _context.BookingSeats.AddRange(bookingSeats);
                await _context.SaveChangesAsync();
            }
        }
    }

    private void SeedSeatsForScreen(Guid screenId, int totalSeats, ScreenType screenType)
    {
        var rows = new[] { "A", "B", "C", "D", "E", "F", "G", "H", "I", "J", "K", "L", "M", "N", "O" };
        var seatsPerRow = Math.Max(10, totalSeats / Math.Min(rows.Length, Math.Max(1, totalSeats / 10)));
        var seatCount = 0;

        // Pricing varies by screen type
        var priceMultiplier = screenType switch
        {
            ScreenType.IMAX => 1.8m,
            ScreenType.Dolby => 1.5m,
            ScreenType.FourDX => 2.0m,
            _ => 1.0m
        };

        for (var r = 0; r < rows.Length && seatCount < totalSeats; r++)
        {
            var category = r < 3 ? SeatCategory.Silver :
                          r < 7 ? SeatCategory.Gold :
                          r < 11 ? SeatCategory.Platinum : SeatCategory.Recliner;

            var price = (category switch
            {
                SeatCategory.Silver => 150m,
                SeatCategory.Gold => 250m,
                SeatCategory.Platinum => 380m,
                SeatCategory.Recliner => 600m,
                _ => 200m
            }) * priceMultiplier;

            for (var s = 1; s <= seatsPerRow && seatCount < totalSeats; s++)
            {
                _context.Seats.Add(new Seat
                {
                    ScreenId = screenId,
                    Row = rows[r],
                    Number = s,
                    Category = category,
                    Price = price,
                    IsActive = true
                });
                seatCount++;
            }
        }
    }
}
