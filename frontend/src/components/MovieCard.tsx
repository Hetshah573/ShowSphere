import { Link } from 'react-router-dom';
import { Star, Clock, Play } from 'lucide-react';
import type { MovieListItem } from '@/types';
import { formatDuration } from '@/lib/utils';

interface MovieCardProps {
  movie: MovieListItem;
}

export function MovieCard({ movie }: MovieCardProps) {
  return (
    <Link to={`/movies/${movie.id}`} className="group relative block rounded-xl overflow-hidden shadow-lg hover:shadow-2xl hover:scale-[1.03] transition-all duration-300 bg-gray-900">
      {/* Poster */}
      <div className="relative aspect-[3/4] overflow-hidden">
        <img
          src={movie.posterUrl || 'https://images.unsplash.com/photo-1489599849927-2ee91cede3ba?w=400&h=600&fit=crop'}
          alt={movie.title}
          className="w-full h-full object-cover group-hover:scale-110 transition-transform duration-500"
        />

        {/* Hover overlay */}
        <div className="absolute inset-0 bg-gradient-to-t from-black via-black/40 to-transparent opacity-0 group-hover:opacity-100 transition-opacity duration-300 flex flex-col justify-end p-4">
          <div className="flex items-center justify-center mb-4">
            <div className="w-12 h-12 rounded-full bg-white/20 backdrop-blur-sm flex items-center justify-center border-2 border-white/60">
              <Play className="h-5 w-5 text-white fill-white ml-0.5" />
            </div>
          </div>
          <p className="text-xs text-gray-300 line-clamp-2">{movie.genres.join(' • ')}</p>
        </div>

        {/* Rating badge */}
        {movie.averageRating > 0 && (
          <div className="absolute top-3 right-3 bg-black/70 backdrop-blur-md px-2.5 py-1 rounded-lg flex items-center gap-1 shadow-lg">
            <Star className="h-3.5 w-3.5 text-yellow-400 fill-yellow-400" />
            <span className="text-sm font-bold text-white">{movie.averageRating.toFixed(1)}</span>
          </div>
        )}

        {/* Genre pills at bottom of image */}
        <div className="absolute bottom-0 left-0 right-0 bg-gradient-to-t from-black/90 via-black/50 to-transparent p-3 pt-8 group-hover:opacity-0 transition-opacity duration-300">
          <div className="flex flex-wrap gap-1.5">
            {movie.genres.slice(0, 2).map((genre) => (
              <span key={genre} className="px-2 py-0.5 rounded-md bg-primary-600/80 text-white text-[11px] font-medium backdrop-blur-sm">
                {genre}
              </span>
            ))}
          </div>
        </div>
      </div>

      {/* Info section */}
      <div className="p-4 bg-white dark:bg-gray-800">
        <h3 className="font-bold text-base truncate text-gray-900 dark:text-white group-hover:text-primary-500 dark:group-hover:text-primary-400 transition-colors">
          {movie.title}
        </h3>
        <div className="flex items-center gap-2.5 mt-2 text-sm text-gray-600 dark:text-gray-400">
          <span className="flex items-center gap-1">
            <Clock className="h-3.5 w-3.5" /> {formatDuration(movie.durationMinutes)}
          </span>
          <span className="w-1 h-1 rounded-full bg-gray-400"></span>
          <span>{movie.language}</span>
          <span className="ml-auto px-2 py-0.5 rounded bg-gray-200 dark:bg-gray-700 text-gray-700 dark:text-gray-300 text-xs font-semibold">{movie.certificate}</span>
        </div>
      </div>
    </Link>
  );
}
