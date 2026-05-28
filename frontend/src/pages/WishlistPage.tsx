import { useState } from 'react';
import { useQuery } from '@tanstack/react-query';
import { wishlistApi } from '@/api';
import { MovieCard } from '@/components/MovieCard';
import { MovieCardSkeleton } from '@/components/Loading';
import { Heart } from 'lucide-react';
import { Link } from 'react-router-dom';

export function WishlistPage() {
  const [page, setPage] = useState(1);

  const { data, isLoading } = useQuery({
    queryKey: ['wishlist', page],
    queryFn: () => wishlistApi.getAll(page),
  });

  const movies = data?.data?.items || [];
  const totalPages = data?.data?.totalPages || 1;

  return (
    <div className="max-w-7xl mx-auto px-4 py-8">
      <h1 className="text-3xl font-bold mb-8 flex items-center gap-3">
        <Heart className="h-8 w-8 text-red-500 fill-red-500" />
        My Wishlist
      </h1>

      {/* Movies Grid */}
      <div className="grid grid-cols-2 sm:grid-cols-3 md:grid-cols-4 lg:grid-cols-6 gap-4">
        {isLoading
          ? Array.from({ length: 6 }).map((_, i) => <MovieCardSkeleton key={i} />)
          : movies.map((movie) => <MovieCard key={movie.id} movie={movie} />)}
      </div>

      {!isLoading && movies.length === 0 && (
        <div className="text-center py-16">
          <Heart className="h-12 w-12 text-gray-400 mx-auto mb-4" />
          <p className="text-gray-600 dark:text-gray-400 text-lg mb-4">Your wishlist is empty</p>
          <Link to="/movies" className="btn-primary">Browse Movies</Link>
        </div>
      )}

      {/* Pagination */}
      {totalPages > 1 && (
        <div className="flex justify-center gap-2 mt-8">
          <button
            onClick={() => setPage((p) => Math.max(1, p - 1))}
            disabled={page === 1}
            className="btn-secondary disabled:opacity-50"
          >
            Previous
          </button>
          <span className="flex items-center px-4 text-gray-600 dark:text-gray-400">
            Page {page} of {totalPages}
          </span>
          <button
            onClick={() => setPage((p) => Math.min(totalPages, p + 1))}
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
