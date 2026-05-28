import { useParams, useNavigate } from 'react-router-dom';
import { useQuery, useMutation, useQueryClient, useInfiniteQuery } from '@tanstack/react-query';
import { moviesApi, showsApi, reviewsApi, wishlistApi } from '@/api';
import { PageLoader } from '@/components/Loading';
import { formatDate, formatTime, formatDuration, formatCurrency } from '@/lib/utils';
import { Star, Clock, Calendar, MapPin, Bell, Heart, Send } from 'lucide-react';
import { useState, useEffect } from 'react';
import { useAuth } from '@/store/AuthContext';
import toast from 'react-hot-toast';

export function MovieDetailPage() {
  const { id } = useParams<{ id: string }>();
  const navigate = useNavigate();
  const [selectedDate, setSelectedDate] = useState<string>(new Date().toISOString().split('T')[0]);

  const { data: movieData, isLoading } = useQuery({
    queryKey: ['movie', id],
    queryFn: () => moviesApi.getById(id!),
    enabled: !!id,
  });

  const { data: showsData } = useQuery({
    queryKey: ['shows', id, selectedDate],
    queryFn: () => showsApi.getByMovie(id!, undefined, selectedDate),
    enabled: !!id,
  });

  const { data: reviewsData, fetchNextPage, hasNextPage, isFetchingNextPage } = useInfiniteQuery({
    queryKey: ['reviews', id],
    queryFn: ({ pageParam = 1 }) => reviewsApi.getByMovie(id!, pageParam),
    getNextPageParam: (lastPage) => {
      const d = lastPage.data;
      return d.page < d.totalPages ? d.page + 1 : undefined;
    },
    initialPageParam: 1,
    enabled: !!id,
  });

  if (isLoading) return <PageLoader />;

  const movie = movieData?.data;
  if (!movie) return <div className="text-center py-20 text-gray-600 dark:text-gray-400">Movie not found</div>;

  const shows = showsData?.data || [];
  const reviews = reviewsData?.pages.flatMap((p) => p.data.items) || [];
  const totalReviews = reviewsData?.pages[0]?.data.totalCount || 0;
  const isUpcoming = new Date(movie.releaseDate) > new Date();

  // Generate next 7 days for date selection
  const dates = Array.from({ length: 7 }, (_, i) => {
    const d = new Date();
    d.setDate(d.getDate() + i);
    return d.toISOString().split('T')[0];
  });

  return (
    <div className="max-w-7xl mx-auto px-4 py-8">
      {/* Movie Header */}
      <div className="flex flex-col md:flex-row gap-8 mb-12">
        <div className="w-full md:w-72 flex-shrink-0">
          <img
            src={movie.posterUrl || '/poster-placeholder.svg'}
            alt={movie.title}
            className="w-full rounded-xl shadow-2xl"
          />
        </div>
        <div className="flex-1">
          <h1 className="text-4xl font-bold mb-3">{movie.title}</h1>
          <div className="flex flex-wrap items-center gap-3 mb-4">
            {movie.averageRating > 0 && (
              <div className="flex items-center gap-1 bg-green-100 dark:bg-green-500/20 text-green-700 dark:text-green-400 px-3 py-1 rounded-lg">
                <Star className="h-4 w-4 fill-current" />
                <span className="font-semibold">{movie.averageRating.toFixed(1)}</span>
                <span className="text-xs text-gray-700 dark:text-gray-400">({movie.totalReviews} reviews)</span>
              </div>
            )}
            <span className="flex items-center gap-1 text-gray-700 dark:text-gray-400">
              <Clock className="h-4 w-4" /> {formatDuration(movie.durationMinutes)}
            </span>
            <span className="badge bg-gray-200 dark:bg-gray-700 text-gray-800 dark:text-gray-300">{movie.certificate}</span>
            <span className="text-gray-700 dark:text-gray-400">{movie.language}</span>
          </div>

          <div className="flex flex-wrap gap-2 mb-4">
            {movie.genres.map((genre) => (
              <span key={genre} className="badge bg-primary-100 dark:bg-primary-500/20 text-primary-700 dark:text-primary-300 px-3 py-1">{genre}</span>
            ))}
          </div>

          <p className="text-gray-800 dark:text-gray-300 leading-relaxed mb-6">{movie.description}</p>

          <div className="text-sm text-gray-700 dark:text-gray-400 flex items-center gap-1">
            <Calendar className="h-4 w-4" />
            {isUpcoming ? `Upcoming on ${formatDate(movie.releaseDate)}` : `Released: ${formatDate(movie.releaseDate)}`}
          </div>

          <WishlistButton movieId={movie.id} />

          {isUpcoming && (
            <NotifyButton movieTitle={movie.title} releaseDate={movie.releaseDate} />
          )}

          {/* Cast */}
          {movie.cast.length > 0 && (
            <div className="mt-6">
              <h3 className="font-semibold mb-3">Cast</h3>
              <div className="flex flex-wrap gap-4">
                {movie.cast.map((member) => (
                  <div key={member.id} className="text-center">
                    <img
                      src={member.photoUrl || '/avatar-placeholder.svg'}
                      alt={member.name}
                      className="w-16 h-16 rounded-full object-cover mx-auto mb-1"
                    />
                    <p className="text-sm font-medium">{member.name}</p>
                    <p className="text-xs text-gray-600 dark:text-gray-400">{member.role}</p>
                  </div>
                ))}
              </div>
            </div>
          )}
        </div>
      </div>

      {/* Shows Section - only for released movies */}
      {!isUpcoming && (
      <div className="mb-12">
        <h2 className="text-2xl font-bold mb-4">Book Tickets</h2>

        {/* Date Selection */}
        <div className="flex gap-2 overflow-x-auto pb-4 mb-6">
          {dates.map((date) => {
            const d = new Date(date);
            const isSelected = date === selectedDate;
            return (
              <button
                key={date}
                onClick={() => setSelectedDate(date)}
                className={`flex-shrink-0 px-4 py-2 rounded-lg text-center transition-all ${
                  isSelected ? 'bg-primary-600 text-white' : 'bg-gray-200 dark:bg-gray-800 text-gray-700 dark:text-gray-300 hover:bg-gray-300 dark:hover:bg-gray-700'
                }`}
              >
                <div className="text-xs">{d.toLocaleDateString('en-US', { weekday: 'short' })}</div>
                <div className="font-semibold">{d.getDate()}</div>
                <div className="text-xs">{d.toLocaleDateString('en-US', { month: 'short' })}</div>
              </button>
            );
          })}
        </div>

        {/* Theater Shows */}
        {shows.length > 0 ? (
          <div className="space-y-4">
            {shows.map((theater, idx) => (
              <div key={idx} className="card p-4">
                <div className="flex items-start justify-between mb-3">
                  <div>
                    <h3 className="font-semibold text-lg">{theater.theaterName}</h3>
                    <p className="text-sm text-gray-600 dark:text-gray-400 flex items-center gap-1">
                      <MapPin className="h-3 w-3" /> {theater.theaterAddress}
                    </p>
                  </div>
                </div>
                <div className="flex flex-wrap gap-3">
                  {theater.shows.map((show) => (
                    <button
                      key={show.showId}
                      onClick={() => navigate(`/booking/seats/${show.showId}`)}
                      className="border border-primary-500/50 hover:bg-primary-500/20 rounded-lg px-4 py-2 text-sm transition-all"
                    >
                      <div className="font-medium text-primary-300">{formatTime(show.startTime)}</div>
                      <div className="text-xs text-gray-600 dark:text-gray-400">
                        {show.screenName} • {show.screenType}
                      </div>
                      <div className="text-xs text-gray-500">
                        {formatCurrency(show.basePrice)} • {show.availableSeats} seats
                      </div>
                    </button>
                  ))}
                </div>
              </div>
            ))}
          </div>
        ) : (
          <p className="text-gray-600 dark:text-gray-400 text-center py-8">No shows available for this date.</p>
        )}
      </div>
      )}

      {/* Reviews */}
      <div>
        <h2 className="text-2xl font-bold mb-4">Reviews {totalReviews > 0 && <span className="text-base font-normal text-gray-500">({totalReviews})</span>}</h2>

        {/* Write a Review */}
        <ReviewForm movieId={id!} />

        {/* Reviews List */}
        {reviews.length > 0 ? (
          <div className="space-y-4 mt-6">
            {reviews.map((review) => (
              <div key={review.id} className="card p-4">
                <div className="flex items-center gap-3 mb-2">
                  <div className="w-8 h-8 bg-primary-600 rounded-full flex items-center justify-center text-sm font-bold text-white">
                    {review.userName.charAt(0)}
                  </div>
                  <div>
                    <p className="font-medium text-sm">{review.userName}</p>
                    <div className="flex items-center gap-1">
                      {Array.from({ length: 5 }).map((_, i) => (
                        <Star key={i} className={`h-3 w-3 ${i < review.rating ? 'text-yellow-400 fill-yellow-400' : 'text-gray-600'}`} />
                      ))}
                    </div>
                  </div>
                  <span className="text-xs text-gray-500 ml-auto">{formatDate(review.createdAt)}</span>
                </div>
                {review.comment && <p className="text-gray-700 dark:text-gray-300 text-sm whitespace-pre-line">{review.comment}</p>}
              </div>
            ))}

            {/* Show More */}
            {hasNextPage && (
              <button
                onClick={() => fetchNextPage()}
                disabled={isFetchingNextPage}
                className="w-full py-3 text-sm font-medium text-primary-600 dark:text-primary-400 hover:bg-primary-50 dark:hover:bg-primary-900/20 rounded-lg border border-primary-200 dark:border-primary-800 transition-colors"
              >
                {isFetchingNextPage ? 'Loading...' : 'Show More Reviews'}
              </button>
            )}
          </div>
        ) : (
          <p className="text-gray-600 dark:text-gray-400 mt-4">No reviews yet. Be the first to review!</p>
        )}
      </div>
    </div>
  );
}

function NotifyButton({ movieTitle, releaseDate }: { movieTitle: string; releaseDate: string }) {
  const { isAuthenticated } = useAuth();
  const { id } = useParams<{ id: string }>();
  const [subscribed, setSubscribed] = useState(false);
  const [loading, setLoading] = useState(false);

  useEffect(() => {
    if (isAuthenticated && id) {
      moviesApi.getNotifyStatus(id).then(res => setSubscribed(res.data.subscribed)).catch(() => {});
    }
  }, [isAuthenticated, id]);

  const handleToggle = async () => {
    if (!isAuthenticated) {
      toast.error('Please login to get notified');
      return;
    }
    setLoading(true);
    try {
      if (subscribed) {
        await moviesApi.unsubscribeNotify(id!);
        setSubscribed(false);
        toast.success('Notification removed');
      } else {
        await moviesApi.subscribeNotify(id!);
        setSubscribed(true);
        toast.success(`We'll email you when ${movieTitle} releases!`);
      }
    } catch {
      toast.error('Something went wrong');
    } finally {
      setLoading(false);
    }
  };

  if (subscribed) {
    return (
      <button
        onClick={handleToggle}
        disabled={loading}
        className="mt-4 flex items-center gap-2 bg-green-100 dark:bg-green-500/10 border border-green-300 dark:border-green-500/30 text-green-700 dark:text-green-400 px-5 py-2.5 rounded-lg transition-colors text-sm font-medium hover:bg-green-200 dark:hover:bg-green-500/20"
      >
        <Bell className="h-4 w-4 fill-current" />
        {loading ? 'Updating...' : `Notifying on Release (${formatDate(releaseDate)})`}
      </button>
    );
  }

  return (
    <button
      onClick={handleToggle}
      disabled={loading}
      className="mt-4 flex items-center gap-2 bg-primary-600 hover:bg-primary-700 text-white px-5 py-2.5 rounded-lg transition-colors text-sm font-medium disabled:opacity-50"
    >
      <Bell className="h-4 w-4" />
      {loading ? 'Subscribing...' : 'Notify Me on Release'}
    </button>
  );
}

function WishlistButton({ movieId }: { movieId: string }) {
  const { isAuthenticated } = useAuth();
  const queryClient = useQueryClient();
  const navigate = useNavigate();

  const { data } = useQuery({
    queryKey: ['wishlist-status', movieId],
    queryFn: () => wishlistApi.isInWishlist(movieId),
    enabled: isAuthenticated,
  });

  const isInWishlist = data?.data?.isInWishlist ?? false;

  const addMutation = useMutation({
    mutationFn: () => wishlistApi.add(movieId),
    onSuccess: () => queryClient.invalidateQueries({ queryKey: ['wishlist-status', movieId] }),
  });

  const removeMutation = useMutation({
    mutationFn: () => wishlistApi.remove(movieId),
    onSuccess: () => queryClient.invalidateQueries({ queryKey: ['wishlist-status', movieId] }),
  });

  const handleToggle = () => {
    if (!isAuthenticated) {
      navigate('/login');
      return;
    }
    if (isInWishlist) {
      removeMutation.mutate();
    } else {
      addMutation.mutate();
    }
  };

  return (
    <button
      onClick={handleToggle}
      className={`mt-4 flex items-center gap-2 px-4 py-2 rounded-lg border transition-colors text-sm font-medium ${
        isInWishlist
          ? 'bg-red-500/10 border-red-500/30 text-red-500 hover:bg-red-500/20'
          : 'border-gray-300 dark:border-gray-600 text-gray-600 dark:text-gray-400 hover:border-red-400 hover:text-red-400'
      }`}
    >
      <Heart className={`h-4 w-4 ${isInWishlist ? 'fill-current' : ''}`} />
      {isInWishlist ? 'In Wishlist' : 'Add to Wishlist'}
    </button>
  );
}

function ReviewForm({ movieId }: { movieId: string }) {
  const { isAuthenticated } = useAuth();
  const queryClient = useQueryClient();
  const navigate = useNavigate();
  const [rating, setRating] = useState(0);
  const [hoverRating, setHoverRating] = useState(0);
  const [comment, setComment] = useState('');
  const [submitting, setSubmitting] = useState(false);

  if (!isAuthenticated) {
    return (
      <div className="card p-4 bg-gray-50 dark:bg-gray-800/50">
        <p className="text-sm text-gray-600 dark:text-gray-400">
          <button onClick={() => navigate('/login')} className="text-primary-600 dark:text-primary-400 font-medium hover:underline">
            Sign in
          </button>
          {' '}to write a review
        </p>
      </div>
    );
  }

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    if (rating === 0) {
      toast.error('Please select a rating');
      return;
    }
    setSubmitting(true);
    try {
      await reviewsApi.create({ movieId, rating, comment: comment.trim() || undefined });
      toast.success('Review posted!');
      setRating(0);
      setComment('');
      queryClient.invalidateQueries({ queryKey: ['reviews', movieId] });
      queryClient.invalidateQueries({ queryKey: ['movie', movieId] });
    } catch (err: unknown) {
      const error = err as { response?: { data?: { error?: string } } };
      toast.error(error.response?.data?.error || 'Failed to post review');
    } finally {
      setSubmitting(false);
    }
  };

  return (
    <form onSubmit={handleSubmit} className="card p-4">
      <p className="text-sm font-medium text-gray-900 dark:text-white mb-3">Write a Review</p>

      {/* Star Rating */}
      <div className="flex items-center gap-1 mb-3">
        {Array.from({ length: 5 }).map((_, i) => (
          <button
            key={i}
            type="button"
            onMouseEnter={() => setHoverRating(i + 1)}
            onMouseLeave={() => setHoverRating(0)}
            onClick={() => setRating(i + 1)}
            className="p-0.5"
          >
            <Star
              className={`h-6 w-6 transition-colors ${
                i < (hoverRating || rating)
                  ? 'text-yellow-400 fill-yellow-400'
                  : 'text-gray-300 dark:text-gray-600'
              }`}
            />
          </button>
        ))}
        {rating > 0 && <span className="text-sm text-gray-500 ml-2">{rating}/5</span>}
      </div>

      {/* Comment */}
      <textarea
        value={comment}
        onChange={(e) => setComment(e.target.value)}
        placeholder="Share your thoughts about the movie... (optional)"
        rows={3}
        maxLength={2000}
        className="w-full px-3 py-2 border border-gray-300 dark:border-gray-600 rounded-lg bg-white dark:bg-gray-700 text-gray-900 dark:text-white text-sm resize-none mb-3"
      />

      <div className="flex items-center justify-between">
        <span className="text-xs text-gray-400">{comment.length}/2000</span>
        <button
          type="submit"
          disabled={submitting || rating === 0}
          className="btn-primary px-4 py-2 text-sm flex items-center gap-2 disabled:opacity-50"
        >
          <Send className="h-3.5 w-3.5" />
          {submitting ? 'Posting...' : 'Post Review'}
        </button>
      </div>
    </form>
  );
}
