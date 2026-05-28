import { useState, useMemo } from 'react';
import { useQuery } from '@tanstack/react-query';
import { moviesApi } from '@/api';
import { MovieCard } from '@/components/MovieCard';
import { MovieCardSkeleton } from '@/components/Loading';
import { Search, Filter, Film, CalendarClock, Layers, X, SlidersHorizontal } from 'lucide-react';

export function MoviesPage() {
  const [search, setSearch] = useState('');
  const [language, setLanguage] = useState('');
  const [genre, setGenre] = useState('');
  const [sortBy, setSortBy] = useState('releasedate');
  const [page, setPage] = useState(1);

  const { data, isLoading } = useQuery({
    queryKey: ['movies', { search, language, genre, sortBy, page }],
    queryFn: () => moviesApi.getAll({ search: search || undefined, language: language || undefined, genre: genre || undefined, sortBy, page, pageSize: 24 }),
  });

  const movies = data?.data?.items || [];
  const totalPages = data?.data?.totalPages || 1;

  const now = new Date();
  const nowShowing = movies.filter((m) => new Date(m.releaseDate) <= now);
  const upcoming = movies.filter((m) => new Date(m.releaseDate) > now);

  const moviesByGenre = useMemo(() => {
    const genreMap: Record<string, typeof movies> = {};
    movies.forEach((movie) => {
      movie.genres.forEach((g) => {
        if (!genreMap[g]) genreMap[g] = [];
        if (!genreMap[g].find((m) => m.id === movie.id)) {
          genreMap[g].push(movie);
        }
      });
    });
    // Sort genres alphabetically, return only genres with 1+ movies
    return Object.entries(genreMap).sort(([a], [b]) => a.localeCompare(b));
  }, [movies]);

  return (
    <div className="w-[90%] mx-auto py-8">
      <h1 className="text-3xl font-bold mb-8 text-gray-900 dark:text-white">Lets Book Your Favourite Movie </h1>

      {/* Search & Filters */}
      <div className="mb-8 space-y-4">
        {/* Main Search Bar */}
        <div className="relative">
          <Search className="absolute left-5 top-1/2 -translate-y-1/2 h-5 w-5 text-gray-400" />
          <input
            type="text"
            value={search}
            onChange={(e) => { setSearch(e.target.value); setPage(1); }}
            className="w-full pl-14 pr-12 py-4 text-lg rounded-2xl border-2 border-gray-200 dark:border-gray-700 bg-white dark:bg-gray-800 text-gray-900 dark:text-white placeholder-gray-400 focus:border-primary-500 focus:ring-4 focus:ring-primary-500/20 outline-none transition-all shadow-sm hover:shadow-md"
            placeholder="Search for movies, genres, languages..."
          />
          {search && (
            <button
              onClick={() => { setSearch(''); setPage(1); }}
              className="absolute right-5 top-1/2 -translate-y-1/2 text-gray-400 hover:text-gray-600 dark:hover:text-gray-300 transition-colors"
            >
              <X className="h-5 w-5" />
            </button>
          )}
        </div>

        {/* Filter Pills */}
        <div className="flex flex-wrap items-center gap-3">
          <span className="flex items-center gap-1.5 text-sm font-medium text-gray-600 dark:text-gray-400">
            <SlidersHorizontal className="h-4 w-4" /> Filters:
          </span>

          <select
            value={language}
            onChange={(e) => { setLanguage(e.target.value); setPage(1); }}
            className="px-4 py-2 rounded-full border border-gray-200 dark:border-gray-700 bg-white dark:bg-gray-800 text-sm text-gray-700 dark:text-gray-300 hover:border-primary-400 focus:border-primary-500 focus:ring-2 focus:ring-primary-500/20 outline-none transition-all cursor-pointer"
          >
            <option value="">All Languages</option>
            <option value="English">English</option>
            <option value="Hindi">Hindi</option>
            <option value="Tamil">Tamil</option>
            <option value="Telugu">Telugu</option>
          </select>

          <select
            value={genre}
            onChange={(e) => { setGenre(e.target.value); setPage(1); }}
            className="px-4 py-2 rounded-full border border-gray-200 dark:border-gray-700 bg-white dark:bg-gray-800 text-sm text-gray-700 dark:text-gray-300 hover:border-primary-400 focus:border-primary-500 focus:ring-2 focus:ring-primary-500/20 outline-none transition-all cursor-pointer"
          >
            <option value="">All Genres</option>
            <option value="Action">Action</option>
            <option value="Drama">Drama</option>
            <option value="Comedy">Comedy</option>
            <option value="Thriller">Thriller</option>
            <option value="Horror">Horror</option>
            <option value="Romance">Romance</option>
            <option value="Sci-Fi">Sci-Fi</option>
            <option value="Adventure">Adventure</option>
          </select>

          <select
            value={sortBy}
            onChange={(e) => { setSortBy(e.target.value); setPage(1); }}
            className="px-4 py-2 rounded-full border border-gray-200 dark:border-gray-700 bg-white dark:bg-gray-800 text-sm text-gray-700 dark:text-gray-300 hover:border-primary-400 focus:border-primary-500 focus:ring-2 focus:ring-primary-500/20 outline-none transition-all cursor-pointer"
          >
            <option value="releasedate">Release Date</option>
            <option value="rating">Top Rated</option>
            <option value="title">A-Z</option>
          </select>

          {(language || genre || search) && (
            <button
              onClick={() => { setSearch(''); setLanguage(''); setGenre(''); setPage(1); }}
              className="px-4 py-2 rounded-full bg-red-50 dark:bg-red-500/10 text-red-600 dark:text-red-400 text-sm font-medium hover:bg-red-100 dark:hover:bg-red-500/20 transition-colors"
            >
              Clear All
            </button>
          )}
        </div>
      </div>

      {isLoading ? (
        <div className="grid grid-cols-2 sm:grid-cols-3 md:grid-cols-4 gap-6">
          {Array.from({ length: 8 }).map((_, i) => <MovieCardSkeleton key={i} />)}
        </div>
      ) : movies.length === 0 ? (
        <div className="text-center py-16">
          <Filter className="h-12 w-12 text-gray-600 mx-auto mb-4" />
          <p className="text-gray-600 dark:text-gray-400 text-lg">No movies found matching your criteria.</p>
        </div>
      ) : (
        <>
          {/* Now Showing Section */}
          {nowShowing.length > 0 && (
            <div className="mb-10">
              <h2 className="text-xl font-bold mb-4 flex items-center gap-2">
                <Film className="h-5 w-5 text-green-500" />
                <span>Now Showing</span>
                <span className="text-sm font-normal text-gray-500 dark:text-gray-400">({nowShowing.length})</span>
              </h2>
              <div className="grid grid-cols-2 sm:grid-cols-3 md:grid-cols-4 gap-6">
                {nowShowing.map((movie) => <MovieCard key={movie.id} movie={movie} />)}
              </div>
            </div>
          )}

          {/* Upcoming Section */}
          {upcoming.length > 0 && (
            <div className="mb-10">
              <h2 className="text-xl font-bold mb-4 flex items-center gap-2">
                <CalendarClock className="h-5 w-5 text-yellow-500" />
                <span>Upcoming</span>
                <span className="text-sm font-normal text-gray-500 dark:text-gray-400">({upcoming.length})</span>
              </h2>
              <div className="grid grid-cols-2 sm:grid-cols-3 md:grid-cols-4 gap-6">
                {upcoming.map((movie) => <MovieCard key={movie.id} movie={movie} />)}
              </div>
            </div>
          )}

          {/* Browse by Genre */}
          {moviesByGenre.length > 0 && (
            <div className="mt-12 border-t border-gray-200 dark:border-gray-700 pt-10">
              <h2 className="text-2xl font-bold mb-8 flex items-center gap-2 text-gray-900 dark:text-white">
                <Layers className="h-6 w-6 text-purple-500" />
                Browse by Genre
              </h2>
              {moviesByGenre.map(([genreName, genreMovies]) => (
                <div key={genreName} className="mb-10">
                  <h3 className="text-lg font-semibold mb-4 text-gray-800 dark:text-gray-200">
                    {genreName}
                    <span className="text-sm font-normal text-gray-500 dark:text-gray-400 ml-2">({genreMovies.length})</span>
                  </h3>
                  <div className="grid grid-cols-2 sm:grid-cols-3 md:grid-cols-4 gap-6">
                    {genreMovies.map((movie) => <MovieCard key={movie.id} movie={movie} />)}
                  </div>
                </div>
              ))}
            </div>
          )}
        </>
      )}

      {/* Pagination */}
      {totalPages > 1 && (
        <div className="flex justify-center gap-2 mt-8">
          <button
            onClick={() => setPage(p => Math.max(1, p - 1))}
            disabled={page === 1}
            className="btn-secondary disabled:opacity-50"
          >
            Previous
          </button>
          <span className="flex items-center px-4 text-gray-600 dark:text-gray-400">
            Page {page} of {totalPages}
          </span>
          <button
            onClick={() => setPage(p => Math.min(totalPages, p + 1))}
            disabled={page === totalPages}
            className="btn-secondary disabled:opacity-50"
          >
            Next
          </button>
        </div>
      )}
    </div>
  );
}
