import { useState, useMemo, useRef, useEffect } from 'react';
import { useQuery } from '@tanstack/react-query';
import { moviesApi, theatersApi } from '@/api';
import { MovieCard } from '@/components/MovieCard';
import { MovieCardSkeleton } from '@/components/Loading';
import { Search, Filter, Film, CalendarClock, Layers, X, SlidersHorizontal, ChevronDown, Check } from 'lucide-react';

// --- Multi-Select Dropdown Component ---
function MultiSelect({ label, options, selected, onChange, placeholder }: {
  label: string;
  options: { value: string; label: string }[];
  selected: string[];
  onChange: (values: string[]) => void;
  placeholder: string;
}) {
  const [open, setOpen] = useState(false);
  const ref = useRef<HTMLDivElement>(null);

  useEffect(() => {
    const handler = (e: MouseEvent) => {
      if (ref.current && !ref.current.contains(e.target as Node)) setOpen(false);
    };
    document.addEventListener('mousedown', handler);
    return () => document.removeEventListener('mousedown', handler);
  }, []);

  const toggle = (value: string) => {
    onChange(selected.includes(value) ? selected.filter(v => v !== value) : [...selected, value]);
  };

  return (
    <div className="space-y-1.5">
      <label className="text-xs font-semibold uppercase tracking-wider text-gray-500 dark:text-gray-400">{label}</label>
      <div ref={ref} className="relative">
        <button
          type="button"
          onClick={() => setOpen(!open)}
          className={`w-full flex items-center justify-between gap-2 px-3.5 py-2.5 rounded-lg border text-sm text-left transition-all ${
            selected.length > 0
              ? 'border-primary-400 dark:border-primary-500/50 bg-primary-50/50 dark:bg-primary-500/5'
              : 'border-gray-200 dark:border-gray-600 bg-white dark:bg-gray-800'
          } hover:border-primary-400 focus:border-primary-500 focus:ring-2 focus:ring-primary-500/20 outline-none`}
        >
          <span className={`truncate ${selected.length > 0 ? 'text-gray-900 dark:text-gray-100' : 'text-gray-500 dark:text-gray-400'}`}>
            {selected.length === 0 ? placeholder : selected.length === 1
              ? options.find(o => o.value === selected[0])?.label || selected[0]
              : `${selected.length} selected`}
          </span>
          <ChevronDown className={`h-4 w-4 shrink-0 text-gray-400 transition-transform ${open ? 'rotate-180' : ''}`} />
        </button>

        {open && (
          <div className="absolute z-[100] mt-1.5 w-full min-w-[220px] max-h-60 overflow-auto rounded-xl border border-gray-200 dark:border-gray-700 bg-white dark:bg-gray-800 shadow-xl shadow-black/15 dark:shadow-black/40 py-1.5">
            {options.map((opt) => {
              const isSelected = selected.includes(opt.value);
              return (
                <button
                  key={opt.value}
                  type="button"
                  onClick={() => toggle(opt.value)}
                  className={`w-full flex items-center gap-2.5 px-3.5 py-2 text-sm transition-colors ${
                    isSelected
                      ? 'bg-primary-50 dark:bg-primary-500/10 text-primary-700 dark:text-primary-300'
                      : 'text-gray-700 dark:text-gray-300 hover:bg-gray-50 dark:hover:bg-gray-700/50'
                  }`}
                >
                  <span className={`flex items-center justify-center w-4 h-4 rounded border transition-all ${
                    isSelected
                      ? 'bg-primary-500 border-primary-500'
                      : 'border-gray-300 dark:border-gray-600'
                  }`}>
                    {isSelected && <Check className="h-3 w-3 text-white" />}
                  </span>
                  <span className="truncate">{opt.label}</span>
                </button>
              );
            })}
          </div>
        )}
      </div>
    </div>
  );
}

// --- Main Page ---
export function MoviesPage() {
  const [search, setSearch] = useState('');
  const [languages, setLanguages] = useState<string[]>([]);
  const [genres, setGenres] = useState<string[]>([]);
  const [sortBy, setSortBy] = useState('releasedate');
  const [timeSlots, setTimeSlots] = useState<string[]>([]);
  const [theaterIds, setTheaterIds] = useState<string[]>([]);
  const [priceRanges, setPriceRanges] = useState<string[]>([]);
  const [page, setPage] = useState(1);
  const [showFilters, setShowFilters] = useState(false);

  // Fetch theaters for dropdown
  const { data: theatersData } = useQuery({
    queryKey: ['theaters'],
    queryFn: () => theatersApi.getAll(),
    staleTime: 5 * 60 * 1000,
  });
  const theaters = theatersData?.data || [];

  // Parse price ranges to min/max (merge ranges)
  const priceParams = useMemo(() => {
    if (priceRanges.length === 0) return {};
    const allMins: number[] = [];
    const allMaxes: number[] = [];
    priceRanges.forEach(r => {
      const [min, max] = r.split('-').map(Number);
      allMins.push(min);
      allMaxes.push(max);
    });
    return { minPrice: Math.min(...allMins) || undefined, maxPrice: Math.max(...allMaxes) || undefined };
  }, [priceRanges]);

  const { data, isLoading } = useQuery({
    queryKey: ['movies', { search, languages, genres, sortBy, page, timeSlots, theaterIds, priceRanges }],
    queryFn: () => moviesApi.getAll({
      search: search || undefined,
      language: languages.join(',') || undefined,
      genre: genres.join(',') || undefined,
      sortBy,
      page,
      pageSize: 24,
      timeSlot: timeSlots.join(',') || undefined,
      theaterId: theaterIds.join(',') || undefined,
      ...priceParams,
      hasAvailableShows: (timeSlots.length || theaterIds.length || priceRanges.length) ? true : undefined,
    }),
  });

  const movies = data?.data?.items || [];
  const totalPages = data?.data?.totalPages || 1;

  const activeFilterCount = [languages, genres, timeSlots, theaterIds, priceRanges].reduce((a, arr) => a + arr.length, 0);

  const clearAll = () => {
    setSearch(''); setLanguages([]); setGenres([]); setTimeSlots([]); setTheaterIds([]); setPriceRanges([]); setSortBy('releasedate'); setPage(1);
  };

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
    return Object.entries(genreMap).sort(([a], [b]) => a.localeCompare(b));
  }, [movies]);

  const languageOptions = [
    { value: 'English', label: 'English' },
    { value: 'Hindi', label: 'Hindi' },
    { value: 'Tamil', label: 'Tamil' },
    { value: 'Telugu', label: 'Telugu' },
  ];

  const genreOptions = [
    { value: 'Action', label: 'Action' },
    { value: 'Drama', label: 'Drama' },
    { value: 'Comedy', label: 'Comedy' },
    { value: 'Thriller', label: 'Thriller' },
    { value: 'Horror', label: 'Horror' },
    { value: 'Romance', label: 'Romance' },
    { value: 'Sci-Fi', label: 'Sci-Fi' },
    { value: 'Adventure', label: 'Adventure' },
  ];

  const timeSlotOptions = [
    { value: 'morning', label: 'Morning (6AM – 12PM)' },
    { value: 'afternoon', label: 'Afternoon (12 – 5PM)' },
    { value: 'evening', label: 'Evening (5 – 9PM)' },
    { value: 'night', label: 'Night (9PM – 6AM)' },
  ];

  const theaterOptions = theaters.map(t => ({ value: t.id, label: `${t.name} — ${t.city}` }));

  const priceOptions = [
    { value: '0-150', label: 'Under ₹150' },
    { value: '150-300', label: '₹150 – ₹300' },
    { value: '300-500', label: '₹300 – ₹500' },
    { value: '500-99999', label: '₹500+' },
  ];

  return (
    <div className="w-full max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 py-6 sm:py-10">
      <h1 className="text-2xl sm:text-3xl lg:text-4xl font-bold mb-6 sm:mb-8 text-gray-900 dark:text-white">
        Let's Book Your Favourite Movie
      </h1>

      {/* Search & Filters */}
      <div className="mb-6 sm:mb-10 space-y-4">
        {/* Search Bar + Filter Toggle */}
        <div className="flex flex-col sm:flex-row gap-3">
          <div className="relative flex-1">
            <Search className="absolute left-4 top-1/2 -translate-y-1/2 h-5 w-5 text-gray-400" />
            <input
              type="text"
              value={search}
              onChange={(e) => { setSearch(e.target.value); setPage(1); }}
              className="w-full pl-12 pr-10 py-3 sm:py-3.5 text-base rounded-xl border border-gray-200 dark:border-gray-700 bg-white dark:bg-gray-800/80 text-gray-900 dark:text-white placeholder-gray-400 focus:border-primary-500 focus:ring-2 focus:ring-primary-500/20 outline-none transition-all backdrop-blur-sm"
              placeholder="Search movies..."
            />
            {search && (
              <button
                onClick={() => { setSearch(''); setPage(1); }}
                className="absolute right-4 top-1/2 -translate-y-1/2 text-gray-400 hover:text-gray-600 dark:hover:text-gray-300"
              >
                <X className="h-4 w-4" />
              </button>
            )}
          </div>

          <button
            onClick={() => setShowFilters(!showFilters)}
            className={`flex items-center justify-center gap-2 px-5 py-3 sm:py-3.5 rounded-xl border text-sm font-medium transition-all ${
              showFilters || activeFilterCount > 0
                ? 'border-primary-500 bg-primary-50 dark:bg-primary-500/10 text-primary-600 dark:text-primary-400'
                : 'border-gray-200 dark:border-gray-700 bg-white dark:bg-gray-800/80 text-gray-700 dark:text-gray-300 hover:border-gray-300 dark:hover:border-gray-600'
            }`}
          >
            <SlidersHorizontal className="h-4 w-4" />
            <span>Filters</span>
            {activeFilterCount > 0 && (
              <span className="ml-1 px-1.5 py-0.5 text-xs rounded-full bg-primary-500 text-white">{activeFilterCount}</span>
            )}
            <ChevronDown className={`h-4 w-4 transition-transform ${showFilters ? 'rotate-180' : ''}`} />
          </button>

          <select
            value={sortBy}
            onChange={(e) => { setSortBy(e.target.value); setPage(1); }}
            className="px-4 py-3 sm:py-3.5 rounded-xl border border-gray-200 dark:border-gray-700 bg-white dark:bg-gray-800/80 text-sm text-gray-700 dark:text-gray-300 outline-none transition-all cursor-pointer focus:border-primary-500 focus:ring-2 focus:ring-primary-500/20"
          >
            <option value="releasedate">Sort: Release Date</option>
            <option value="rating">Sort: Top Rated</option>
            <option value="title">Sort: A-Z</option>
          </select>
        </div>

        {/* Expandable Filter Panel */}
        {showFilters && (
          <div className="p-4 sm:p-5 rounded-2xl border border-gray-200 dark:border-gray-700/50 bg-gray-50/80 dark:bg-gray-800/40 backdrop-blur-sm relative z-10">
            <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-5 gap-3 sm:gap-4 overflow-visible">
              <MultiSelect label="Language" options={languageOptions} selected={languages} onChange={(v) => { setLanguages(v); setPage(1); }} placeholder="All Languages" />
              <MultiSelect label="Genre" options={genreOptions} selected={genres} onChange={(v) => { setGenres(v); setPage(1); }} placeholder="All Genres" />
              <MultiSelect label="Show Time" options={timeSlotOptions} selected={timeSlots} onChange={(v) => { setTimeSlots(v); setPage(1); }} placeholder="Any Time" />
              <MultiSelect label="Theater" options={theaterOptions} selected={theaterIds} onChange={(v) => { setTheaterIds(v); setPage(1); }} placeholder="All Theaters" />
              <MultiSelect label="Price" options={priceOptions} selected={priceRanges} onChange={(v) => { setPriceRanges(v); setPage(1); }} placeholder="Any Price" />
            </div>

            {/* Active filters + Clear */}
            {activeFilterCount > 0 && (
              <div className="mt-4 pt-3 border-t border-gray-200 dark:border-gray-700/50 flex flex-wrap items-center gap-2">
                <span className="text-xs text-gray-500 dark:text-gray-400">Active:</span>
                {languages.map(l => <FilterChip key={l} label={l} onRemove={() => setLanguages(languages.filter(v => v !== l))} />)}
                {genres.map(g => <FilterChip key={g} label={g} onRemove={() => setGenres(genres.filter(v => v !== g))} />)}
                {timeSlots.map(t => <FilterChip key={t} label={t} onRemove={() => setTimeSlots(timeSlots.filter(v => v !== t))} />)}
                {theaterIds.map(id => <FilterChip key={id} label={theaters.find(t => t.id === id)?.name || 'Theater'} onRemove={() => setTheaterIds(theaterIds.filter(v => v !== id))} />)}
                {priceRanges.map(p => <FilterChip key={p} label={p === '500-99999' ? '₹500+' : `₹${p.replace('-', '–₹')}`} onRemove={() => setPriceRanges(priceRanges.filter(v => v !== p))} />)}
                <button
                  onClick={clearAll}
                  className="ml-auto text-xs font-medium text-red-500 hover:text-red-600 dark:text-red-400 dark:hover:text-red-300 transition-colors"
                >
                  Clear all
                </button>
              </div>
            )}
          </div>
        )}
      </div>

      {isLoading ? (
        <div className="grid grid-cols-2 sm:grid-cols-3 lg:grid-cols-4 xl:grid-cols-5 gap-4 sm:gap-6">
          {Array.from({ length: 10 }).map((_, i) => <MovieCardSkeleton key={i} />)}
        </div>
      ) : movies.length === 0 ? (
        <div className="text-center py-20">
          <Filter className="h-12 w-12 text-gray-400 dark:text-gray-600 mx-auto mb-4" />
          <p className="text-gray-500 dark:text-gray-400 text-lg font-medium">No movies found</p>
          <p className="text-gray-400 dark:text-gray-500 text-sm mt-1">Try adjusting your filters or search term</p>
          {activeFilterCount > 0 && (
            <button onClick={clearAll} className="mt-4 text-sm text-primary-500 hover:text-primary-600 font-medium">
              Clear all filters
            </button>
          )}
        </div>
      ) : (
        <>
          {nowShowing.length > 0 && (
            <div className="mb-10">
              <h2 className="text-xl font-bold mb-4 flex items-center gap-2">
                <Film className="h-5 w-5 text-green-500" />
                <span>Now Showing</span>
                <span className="text-sm font-normal text-gray-500 dark:text-gray-400">({nowShowing.length})</span>
              </h2>
              <div className="grid grid-cols-2 sm:grid-cols-3 lg:grid-cols-4 xl:grid-cols-5 gap-4 sm:gap-6">
                {nowShowing.map((movie) => <MovieCard key={movie.id} movie={movie} />)}
              </div>
            </div>
          )}

          {upcoming.length > 0 && (
            <div className="mb-10">
              <h2 className="text-xl font-bold mb-4 flex items-center gap-2">
                <CalendarClock className="h-5 w-5 text-yellow-500" />
                <span>Upcoming</span>
                <span className="text-sm font-normal text-gray-500 dark:text-gray-400">({upcoming.length})</span>
              </h2>
              <div className="grid grid-cols-2 sm:grid-cols-3 lg:grid-cols-4 xl:grid-cols-5 gap-4 sm:gap-6">
                {upcoming.map((movie) => <MovieCard key={movie.id} movie={movie} />)}
              </div>
            </div>
          )}

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
                  <div className="grid grid-cols-2 sm:grid-cols-3 lg:grid-cols-4 xl:grid-cols-5 gap-4 sm:gap-6">
                    {genreMovies.map((movie) => <MovieCard key={movie.id} movie={movie} />)}
                  </div>
                </div>
              ))}
            </div>
          )}
        </>
      )}

      {totalPages > 1 && (
        <div className="flex justify-center items-center gap-2 mt-10">
          <button
            onClick={() => setPage(p => Math.max(1, p - 1))}
            disabled={page === 1}
            className="px-4 py-2 rounded-lg border border-gray-200 dark:border-gray-700 bg-white dark:bg-gray-800 text-sm font-medium text-gray-700 dark:text-gray-300 disabled:opacity-40 hover:bg-gray-50 dark:hover:bg-gray-700 transition-colors"
          >
            Previous
          </button>
          <span className="px-4 py-2 text-sm text-gray-600 dark:text-gray-400">
            {page} / {totalPages}
          </span>
          <button
            onClick={() => setPage(p => Math.min(totalPages, p + 1))}
            disabled={page === totalPages}
            className="px-4 py-2 rounded-lg border border-gray-200 dark:border-gray-700 bg-white dark:bg-gray-800 text-sm font-medium text-gray-700 dark:text-gray-300 disabled:opacity-40 hover:bg-gray-50 dark:hover:bg-gray-700 transition-colors"
          >
            Next
          </button>
        </div>
      )}
    </div>
  );
}

function FilterChip({ label, onRemove }: { label: string; onRemove: () => void }) {
  return (
    <span className="inline-flex items-center gap-1 px-2.5 py-1 rounded-full bg-primary-100 dark:bg-primary-500/15 text-primary-700 dark:text-primary-300 text-xs font-medium capitalize">
      {label}
      <button onClick={onRemove} className="hover:text-primary-900 dark:hover:text-white transition-colors">
        <X className="h-3 w-3" />
      </button>
    </span>
  );
}
