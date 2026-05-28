import { useState } from 'react';
import { useQuery, useQueryClient } from '@tanstack/react-query';
import { adminApi, moviesApi } from '@/api';
import { Plus, Trash2, X, Search } from 'lucide-react';
import { formatCurrency } from '@/lib/utils';
import toast from 'react-hot-toast';

export function AdminShowsPage() {
  const queryClient = useQueryClient();
  const [page, setPage] = useState(1);
  const [showForm, setShowForm] = useState(false);

  const { data: showsData, isLoading } = useQuery({
    queryKey: ['admin-shows', page],
    queryFn: () => adminApi.getShows(page),
  });

  const shows = showsData?.data;

  const handleDelete = async (showId: string) => {
    if (!confirm('Delete this show? Only shows without confirmed bookings can be deleted.')) return;
    try {
      await adminApi.deleteShow(showId);
      toast.success('Show deleted');
      queryClient.invalidateQueries({ queryKey: ['admin-shows'] });
    } catch (err: unknown) {
      const error = err as { response?: { data?: { error?: string } } };
      toast.error(error.response?.data?.error || 'Failed to delete show');
    }
  };

  return (
    <div className="max-w-7xl mx-auto px-4 py-8">
      <div className="flex items-center justify-between mb-6">
        <h1 className="text-2xl font-bold text-gray-900 dark:text-white">Manage Shows</h1>
        <button onClick={() => setShowForm(true)} className="btn-primary flex items-center gap-2">
          <Plus className="h-4 w-4" /> Schedule Show
        </button>
      </div>

      {/* Table */}
      <div className="bg-white dark:bg-gray-800 rounded-xl shadow overflow-hidden">
        <div className="overflow-x-auto">
          <table className="w-full text-left text-sm">
            <thead className="bg-gray-50 dark:bg-gray-700">
              <tr>
                <th className="px-4 py-3 text-xs font-medium text-gray-500 dark:text-gray-400 uppercase">Movie</th>
                <th className="px-4 py-3 text-xs font-medium text-gray-500 dark:text-gray-400 uppercase">Theater</th>
                <th className="px-4 py-3 text-xs font-medium text-gray-500 dark:text-gray-400 uppercase">Screen</th>
                <th className="px-4 py-3 text-xs font-medium text-gray-500 dark:text-gray-400 uppercase">Date & Time</th>
                <th className="px-4 py-3 text-xs font-medium text-gray-500 dark:text-gray-400 uppercase">Price</th>
                <th className="px-4 py-3 text-xs font-medium text-gray-500 dark:text-gray-400 uppercase">Seats</th>
                <th className="px-4 py-3 text-xs font-medium text-gray-500 dark:text-gray-400 uppercase">Actions</th>
              </tr>
            </thead>
            <tbody className="divide-y divide-gray-200 dark:divide-gray-700">
              {isLoading ? (
                <tr><td colSpan={7} className="px-4 py-8 text-center text-gray-500">Loading...</td></tr>
              ) : !shows?.items?.length ? (
                <tr><td colSpan={7} className="px-4 py-8 text-center text-gray-500">No shows scheduled</td></tr>
              ) : (
                shows.items.map((show) => (
                  <tr key={show.id} className="hover:bg-gray-50 dark:hover:bg-gray-750">
                    <td className="px-4 py-3 font-medium text-gray-900 dark:text-white">{show.movieTitle}</td>
                    <td className="px-4 py-3 text-gray-700 dark:text-gray-300">
                      {show.theaterName}
                      <span className="block text-xs text-gray-500">{show.city}</span>
                    </td>
                    <td className="px-4 py-3 text-gray-700 dark:text-gray-300">{show.screenName}</td>
                    <td className="px-4 py-3 text-gray-700 dark:text-gray-300">
                      {new Date(show.startTime).toLocaleDateString('en-IN', { day: 'numeric', month: 'short' })}
                      <span className="block text-xs text-gray-500">
                        {new Date(show.startTime).toLocaleTimeString('en-IN', { hour: '2-digit', minute: '2-digit' })}
                      </span>
                    </td>
                    <td className="px-4 py-3 text-gray-700 dark:text-gray-300">{formatCurrency(show.basePrice)}</td>
                    <td className="px-4 py-3 text-gray-700 dark:text-gray-300">{show.totalSeats}</td>
                    <td className="px-4 py-3">
                      <button
                        onClick={() => handleDelete(show.id)}
                        className="p-1.5 text-red-600 hover:bg-red-50 dark:hover:bg-red-900/20 rounded"
                        title="Delete"
                      >
                        <Trash2 className="h-4 w-4" />
                      </button>
                    </td>
                  </tr>
                ))
              )}
            </tbody>
          </table>
        </div>

        {shows && shows.totalPages > 1 && (
          <div className="flex items-center justify-between px-4 py-3 border-t border-gray-200 dark:border-gray-700">
            <p className="text-sm text-gray-500">Page {shows.page} of {shows.totalPages}</p>
            <div className="flex gap-2">
              <button onClick={() => setPage((p) => Math.max(1, p - 1))} disabled={page === 1} className="px-3 py-1 text-sm border rounded disabled:opacity-50 dark:border-gray-600 dark:text-gray-300">Prev</button>
              <button onClick={() => setPage((p) => p + 1)} disabled={page >= shows.totalPages} className="px-3 py-1 text-sm border rounded disabled:opacity-50 dark:border-gray-600 dark:text-gray-300">Next</button>
            </div>
          </div>
        )}
      </div>

      {/* Create Show Modal */}
      {showForm && <CreateShowModal onClose={() => setShowForm(false)} />}
    </div>
  );
}

function CreateShowModal({ onClose }: { onClose: () => void }) {
  const queryClient = useQueryClient();
  const [movieSearch, setMovieSearch] = useState('');
  const [movieId, setMovieId] = useState('');
  const [theaterId, setTheaterId] = useState('');
  const [screenId, setScreenId] = useState('');
  const [date, setDate] = useState(new Date().toISOString().split('T')[0]);
  const [time, setTime] = useState('12:00');
  const [basePrice, setBasePrice] = useState(200);
  const [saving, setSaving] = useState(false);

  const { data: moviesData } = useQuery({
    queryKey: ['admin-movies-search', movieSearch],
    queryFn: () => moviesApi.getAll({ Search: movieSearch || undefined, PageSize: 10 }),
    enabled: movieSearch.length > 1,
  });

  const { data: theatersData } = useQuery({
    queryKey: ['theaters-list'],
    queryFn: () => adminApi.getTheaters(),
  });

  const { data: theaterData } = useQuery({
    queryKey: ['theater-detail', theaterId],
    queryFn: () => adminApi.getTheater(theaterId),
    enabled: !!theaterId,
  });

  const movies = moviesData?.data?.items || [];
  const theaters = theatersData?.data || [];
  const screens = theaterData?.data?.screens || [];

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    if (!movieId || !screenId) {
      toast.error('Select movie and screen');
      return;
    }
    setSaving(true);
    try {
      const startTime = new Date(`${date}T${time}:00`).toISOString();
      await adminApi.createShow({ movieId, screenId, startTime, basePrice });
      toast.success('Show scheduled!');
      queryClient.invalidateQueries({ queryKey: ['admin-shows'] });
      onClose();
    } catch (err: unknown) {
      const error = err as { response?: { data?: { error?: string } } };
      toast.error(error.response?.data?.error || 'Failed to create show');
    } finally {
      setSaving(false);
    }
  };

  return (
    <div className="fixed inset-0 z-50 flex items-center justify-center bg-black/50 p-4">
      <div className="bg-white dark:bg-gray-800 rounded-xl shadow-xl w-full max-w-lg max-h-[90vh] overflow-y-auto">
        <div className="flex items-center justify-between px-6 py-4 border-b dark:border-gray-700">
          <h2 className="text-lg font-bold text-gray-900 dark:text-white">Schedule a Show</h2>
          <button onClick={onClose} className="text-gray-400 hover:text-gray-600"><X className="h-5 w-5" /></button>
        </div>

        <form onSubmit={handleSubmit} className="p-6 space-y-4">
          {/* Movie Search */}
          <div>
            <label className="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-1">Movie *</label>
            <div className="relative">
              <Search className="absolute left-3 top-1/2 -translate-y-1/2 h-4 w-4 text-gray-400" />
              <input
                type="text"
                value={movieSearch}
                onChange={(e) => { setMovieSearch(e.target.value); setMovieId(''); }}
                placeholder="Search movie..."
                className="w-full pl-10 pr-4 py-2 border rounded-lg dark:border-gray-600 dark:bg-gray-700 dark:text-white"
              />
            </div>
            {movieSearch && !movieId && movies.length > 0 && (
              <div className="mt-1 border rounded-lg dark:border-gray-600 max-h-32 overflow-y-auto">
                {movies.map((m) => (
                  <button
                    key={m.id}
                    type="button"
                    onClick={() => { setMovieId(m.id); setMovieSearch(m.title); }}
                    className="w-full text-left px-3 py-2 hover:bg-gray-100 dark:hover:bg-gray-700 text-sm"
                  >
                    {m.title} ({m.language})
                  </button>
                ))}
              </div>
            )}
            {movieId && <p className="text-xs text-green-600 mt-1">✓ Selected</p>}
          </div>

          {/* Theater */}
          <div>
            <label className="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-1">Theater *</label>
            <select
              value={theaterId}
              onChange={(e) => { setTheaterId(e.target.value); setScreenId(''); }}
              className="w-full px-3 py-2 border rounded-lg dark:border-gray-600 dark:bg-gray-700 dark:text-white"
            >
              <option value="">Select theater...</option>
              {theaters.map((t) => (
                <option key={t.id} value={t.id}>{t.name} — {t.city}</option>
              ))}
            </select>
          </div>

          {/* Screen */}
          {theaterId && (
            <div>
              <label className="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-1">Screen *</label>
              <select
                value={screenId}
                onChange={(e) => setScreenId(e.target.value)}
                className="w-full px-3 py-2 border rounded-lg dark:border-gray-600 dark:bg-gray-700 dark:text-white"
              >
                <option value="">Select screen...</option>
                {screens.map((s) => (
                  <option key={s.id} value={s.id}>{s.name} ({s.screenType}) — {s.totalSeats} seats</option>
                ))}
              </select>
            </div>
          )}

          {/* Date & Time */}
          <div className="grid grid-cols-2 gap-4">
            <div>
              <label className="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-1">Date</label>
              <input
                type="date"
                value={date}
                onChange={(e) => setDate(e.target.value)}
                className="w-full px-3 py-2 border rounded-lg dark:border-gray-600 dark:bg-gray-700 dark:text-white"
              />
            </div>
            <div>
              <label className="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-1">Time</label>
              <input
                type="time"
                value={time}
                onChange={(e) => setTime(e.target.value)}
                className="w-full px-3 py-2 border rounded-lg dark:border-gray-600 dark:bg-gray-700 dark:text-white"
              />
            </div>
          </div>

          {/* Base Price */}
          <div>
            <label className="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-1">Base Price (₹)</label>
            <input
              type="number"
              value={basePrice}
              onChange={(e) => setBasePrice(parseInt(e.target.value) || 0)}
              min={1}
              className="w-full px-3 py-2 border rounded-lg dark:border-gray-600 dark:bg-gray-700 dark:text-white"
            />
          </div>

          {/* Actions */}
          <div className="flex justify-end gap-3 pt-4 border-t dark:border-gray-700">
            <button type="button" onClick={onClose} className="px-4 py-2 text-sm border rounded-lg dark:border-gray-600 dark:text-gray-300">Cancel</button>
            <button type="submit" disabled={saving} className="btn-primary px-6 py-2 disabled:opacity-50">
              {saving ? 'Scheduling...' : 'Schedule Show'}
            </button>
          </div>
        </form>
      </div>
    </div>
  );
}
