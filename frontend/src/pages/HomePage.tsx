import { useQuery } from '@tanstack/react-query';
import { Link } from 'react-router-dom';
import { moviesApi } from '@/api';
import { MovieCard } from '@/components/MovieCard';
import { MovieCardSkeleton } from '@/components/Loading';
import { HeroCarousel } from '@/components/HeroCarousel';
import { TrendingUp, Calendar, ChevronRight } from 'lucide-react';

export function HomePage() {
  const { data: nowShowing, isLoading: loadingNow } = useQuery({
    queryKey: ['movies', 'now-showing'],
    queryFn: () => moviesApi.getNowShowing(undefined, 1),
  });

  const { data: upcoming, isLoading: loadingUpcoming } = useQuery({
    queryKey: ['movies', 'upcoming'],
    queryFn: () => moviesApi.getUpcoming(1),
  });

  return (
    <div className="min-h-screen bg-gray-50 dark:bg-[#141414]">
      {/* Hero Carousel */}
      {loadingNow ? (
        <div className="h-[350px] md:h-[450px] lg:h-[500px] bg-gray-900 animate-pulse" />
      ) : (
        <HeroCarousel movies={nowShowing?.data?.items?.slice(0, 8) || []} />
      )}

      {/* Now Showing */}
      <section className="w-[90%] mx-auto py-12">
        <div className="flex items-center justify-between mb-8">
          <h2 className="text-3xl font-bold flex items-center gap-3 text-gray-900 dark:text-white">
            <TrendingUp className="h-7 w-7 text-red-500" />
            Now Showing
            {nowShowing?.data?.items?.length && (
              <span className="text-base font-normal text-gray-500 dark:text-gray-400">({nowShowing.data.items.length})</span>
            )}
          </h2>
          <Link to="/movies" className="px-4 py-2 rounded-full bg-primary-600 hover:bg-primary-700 text-white text-sm font-medium flex items-center gap-1 transition-colors">
            View All <ChevronRight className="h-4 w-4" />
          </Link>
        </div>
        <div className="grid grid-cols-2 sm:grid-cols-3 md:grid-cols-4 gap-6">
          {loadingNow
            ? Array.from({ length: 4 }).map((_, i) => <MovieCardSkeleton key={i} />)
            : nowShowing?.data?.items?.map((movie) => <MovieCard key={movie.id} movie={movie} />)}
          {!loadingNow && (!nowShowing?.data?.items?.length) && (
            <p className="col-span-full text-center text-gray-500 dark:text-gray-400 py-8">No movies currently showing.</p>
          )}
        </div>
      </section>

      {/* Upcoming */}
      <section className="w-[90%] mx-auto py-12">
        <div className="flex items-center justify-between mb-8">
          <h2 className="text-3xl font-bold flex items-center gap-3 text-gray-900 dark:text-white">
            <Calendar className="h-7 w-7 text-amber-500" />
            Coming Soon
            {upcoming?.data?.items?.length && (
              <span className="text-base font-normal text-gray-500 dark:text-gray-400">({upcoming.data.items.length})</span>
            )}
          </h2>
        </div>
        <div className="grid grid-cols-2 sm:grid-cols-3 md:grid-cols-4 gap-6">
          {loadingUpcoming
            ? Array.from({ length: 4 }).map((_, i) => <MovieCardSkeleton key={i} />)
            : upcoming?.data?.items?.map((movie) => <MovieCard key={movie.id} movie={movie} />)}
          {!loadingUpcoming && (!upcoming?.data?.items?.length) && (
            <p className="col-span-full text-center text-gray-500 dark:text-gray-400 py-8">No upcoming movies.</p>
          )}
        </div>
      </section>
    </div>
  );
}
