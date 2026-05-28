import { useState, useEffect, useCallback } from 'react';
import { Link } from 'react-router-dom';
import { ChevronLeft, ChevronRight } from 'lucide-react';
import { MovieListItem } from '@/types';

interface HeroCarouselProps {
  movies: MovieListItem[];
}

export function HeroCarousel({ movies }: HeroCarouselProps) {
  const [current, setCurrent] = useState(0);

  const next = useCallback(() => {
    setCurrent((prev) => (prev + 1) % movies.length);
  }, [movies.length]);

  const prev = useCallback(() => {
    setCurrent((prev) => (prev - 1 + movies.length) % movies.length);
  }, [movies.length]);

  // Auto-slide every 5 seconds
  useEffect(() => {
    if (movies.length <= 1) return;
    const interval = setInterval(next, 5000);
    return () => clearInterval(interval);
  }, [next, movies.length]);

  if (!movies.length) return null;

  const movie = movies[current];

  return (
    <section className="relative w-full overflow-hidden bg-black">
      {/* Slides */}
      <Link to={`/movies/${movie.id}`} className="block relative h-[350px] md:h-[450px] lg:h-[500px]">
        {/* Background poster */}
        <div className="absolute inset-0">
          {movie.posterUrl ? (
            <img
              src={movie.posterUrl}
              alt={movie.title}
              className="w-full h-full object-cover opacity-40 transition-all duration-700"
            />
          ) : (
            <div className="w-full h-full bg-gradient-to-br from-primary-900 to-gray-900" />
          )}
          <div className="absolute inset-0 bg-gradient-to-t from-black via-black/60 to-transparent" />
          <div className="absolute inset-0 bg-gradient-to-r from-black/80 via-transparent to-black/80" />
        </div>

        {/* Content */}
        <div className="relative h-full flex items-center">
          <div className="max-w-7xl mx-auto px-4 w-full flex items-center gap-8">
            {/* Poster thumbnail */}
            {movie.posterUrl && (
              <div className="hidden md:block flex-shrink-0">
                <img
                  src={movie.posterUrl}
                  alt={movie.title}
                  className="w-48 lg:w-56 rounded-xl shadow-2xl border-2 border-white/20"
                />
              </div>
            )}

            {/* Details */}
            <div className="flex-1 text-white">
              <div className="flex flex-wrap items-center gap-2 mb-3">
                {movie.genres.slice(0, 3).map((genre) => (
                  <span
                    key={genre}
                    className="px-2 py-0.5 bg-primary-600/80 text-white text-xs rounded-full"
                  >
                    {genre}
                  </span>
                ))}
                <span className="px-2 py-0.5 bg-white/20 text-white text-xs rounded-full">
                  {movie.language}
                </span>
                <span className="px-2 py-0.5 bg-white/20 text-white text-xs rounded-full">
                  {movie.certificate}
                </span>
              </div>
              <h2 className="text-3xl md:text-4xl lg:text-5xl font-bold mb-3 drop-shadow-lg">
                {movie.title}
              </h2>
              <div className="flex items-center gap-4 text-sm text-gray-300 mb-4">
                <span>{movie.durationMinutes} min</span>
                {movie.averageRating > 0 && (
                  <span className="flex items-center gap-1">
                    <span className="text-yellow-400">★</span> {movie.averageRating.toFixed(1)}
                  </span>
                )}
                <span>{new Date(movie.releaseDate).toLocaleDateString('en-IN', { day: 'numeric', month: 'short', year: 'numeric' })}</span>
              </div>
              <span className="inline-block bg-primary-600 hover:bg-primary-700 text-white px-6 py-2 rounded-lg font-medium transition-colors">
                Book Now
              </span>
            </div>
          </div>
        </div>
      </Link>

      {/* Browse Movies button overlay */}
      <div className="absolute top-4 right-4 z-20">
        <Link
          to="/movies"
          className="bg-white/20 backdrop-blur-sm hover:bg-white/30 text-white px-4 py-2 rounded-lg text-sm font-medium transition-colors border border-white/30"
        >
          Browse Movies
        </Link>
      </div>

      {/* Left/Right Arrows */}
      {movies.length > 1 && (
        <>
          <button
            onClick={(e) => { e.preventDefault(); prev(); }}
            className="absolute left-3 top-1/2 -translate-y-1/2 z-20 bg-black/50 hover:bg-black/70 text-white p-2 rounded-full transition-colors"
            aria-label="Previous"
          >
            <ChevronLeft className="h-6 w-6" />
          </button>
          <button
            onClick={(e) => { e.preventDefault(); next(); }}
            className="absolute right-3 top-1/2 -translate-y-1/2 z-20 bg-black/50 hover:bg-black/70 text-white p-2 rounded-full transition-colors"
            aria-label="Next"
          >
            <ChevronRight className="h-6 w-6" />
          </button>
        </>
      )}

      {/* Dots indicator */}
      {movies.length > 1 && (
        <div className="absolute bottom-4 left-1/2 -translate-x-1/2 z-20 flex gap-2">
          {movies.map((_, idx) => (
            <button
              key={idx}
              onClick={(e) => { e.preventDefault(); setCurrent(idx); }}
              className={`w-2.5 h-2.5 rounded-full transition-all ${
                idx === current ? 'bg-primary-500 w-6' : 'bg-white/50 hover:bg-white/80'
              }`}
              aria-label={`Go to slide ${idx + 1}`}
            />
          ))}
        </div>
      )}
    </section>
  );
}
