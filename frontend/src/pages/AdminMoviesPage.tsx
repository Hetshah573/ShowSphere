import { useState } from 'react';
import { useQuery, useQueryClient } from '@tanstack/react-query';
import { moviesApi, adminApi } from '@/api';
import { Movie } from '@/types';
import { Plus, Pencil, Trash2, X, Search } from 'lucide-react';
import toast from 'react-hot-toast';

const CERTIFICATES = [
  { value: 0, label: 'U' },
  { value: 1, label: 'UA' },
  { value: 2, label: 'A' },
  { value: 3, label: 'S' },
];

interface MovieForm {
  title: string;
  description: string;
  posterUrl: string;
  trailerUrl: string;
  durationMinutes: number;
  language: string;
  certificate: number;
  releaseDate: string;
  isActive: boolean;
  genreIds: number[];
}

const emptyForm: MovieForm = {
  title: '',
  description: '',
  posterUrl: '',
  trailerUrl: '',
  durationMinutes: 120,
  language: 'Hindi',
  certificate: 1,
  releaseDate: new Date().toISOString().split('T')[0],
  isActive: true,
  genreIds: [],
};

export function AdminMoviesPage() {
  const queryClient = useQueryClient();
  const [search, setSearch] = useState('');
  const [page, setPage] = useState(1);
  const [showForm, setShowForm] = useState(false);
  const [editingId, setEditingId] = useState<string | null>(null);
  const [form, setForm] = useState<MovieForm>(emptyForm);
  const [saving, setSaving] = useState(false);

  const { data: moviesData, isLoading } = useQuery({
    queryKey: ['admin-movies', search, page],
    queryFn: () => moviesApi.getAll({ Search: search || undefined, Page: page, PageSize: 10 }),
  });

  const { data: genresData } = useQuery({
    queryKey: ['genres'],
    queryFn: () => adminApi.getGenres(),
  });

  const genres = genresData?.data || [];
  const movies = moviesData?.data;

  const openCreate = () => {
    setEditingId(null);
    setForm(emptyForm);
    setShowForm(true);
  };

  const openEdit = async (movieId: string) => {
    try {
      const res = await moviesApi.getById(movieId);
      const m: Movie = res.data;
      const certIdx = CERTIFICATES.findIndex((c) => c.label === m.certificate);
      const genreIds = genres
        .filter((g) => m.genres.includes(g.name))
        .map((g) => g.id);
      setForm({
        title: m.title,
        description: m.description,
        posterUrl: m.posterUrl || '',
        trailerUrl: m.trailerUrl || '',
        durationMinutes: m.durationMinutes,
        language: m.language,
        certificate: certIdx >= 0 ? certIdx : 1,
        releaseDate: m.releaseDate.split('T')[0],
        isActive: true,
        genreIds,
      });
      setEditingId(movieId);
      setShowForm(true);
    } catch {
      toast.error('Failed to load movie details');
    }
  };

  const handleDelete = async (movieId: string, title: string) => {
    if (!confirm(`Delete "${title}"? This will deactivate the movie.`)) return;
    try {
      await adminApi.deleteMovie(movieId);
      toast.success('Movie deleted');
      queryClient.invalidateQueries({ queryKey: ['admin-movies'] });
      queryClient.invalidateQueries({ queryKey: ['movies'] });
    } catch {
      toast.error('Failed to delete movie');
    }
  };

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    if (!form.title.trim() || !form.description.trim()) {
      toast.error('Title and description are required');
      return;
    }
    setSaving(true);
    try {
      if (editingId) {
        await adminApi.updateMovie(editingId, {
          title: form.title,
          description: form.description,
          posterUrl: form.posterUrl || undefined,
          trailerUrl: form.trailerUrl || undefined,
          durationMinutes: form.durationMinutes,
          language: form.language,
          certificate: form.certificate,
          releaseDate: form.releaseDate,
          isActive: form.isActive,
          genreIds: form.genreIds,
        });
        toast.success('Movie updated');
      } else {
        await adminApi.createMovie({
          title: form.title,
          description: form.description,
          posterUrl: form.posterUrl || undefined,
          trailerUrl: form.trailerUrl || undefined,
          durationMinutes: form.durationMinutes,
          language: form.language,
          certificate: form.certificate,
          releaseDate: form.releaseDate,
          genreIds: form.genreIds,
          cast: [],
        });
        toast.success('Movie created');
      }
      setShowForm(false);
      queryClient.invalidateQueries({ queryKey: ['admin-movies'] });
      queryClient.invalidateQueries({ queryKey: ['movies'] });
    } catch (err: unknown) {
      const error = err as { response?: { data?: { error?: string } } };
      toast.error(error.response?.data?.error || 'Operation failed');
    } finally {
      setSaving(false);
    }
  };

  const toggleGenre = (genreId: number) => {
    setForm((f) => ({
      ...f,
      genreIds: f.genreIds.includes(genreId)
        ? f.genreIds.filter((id) => id !== genreId)
        : [...f.genreIds, genreId],
    }));
  };

  return (
    <div className="max-w-7xl mx-auto px-4 py-8">
      <div className="flex items-center justify-between mb-6">
        <h1 className="text-2xl font-bold text-gray-900 dark:text-white">Manage Movies</h1>
        <button onClick={openCreate} className="btn-primary flex items-center gap-2">
          <Plus className="h-4 w-4" /> Add Movie
        </button>
      </div>

      {/* Search */}
      <div className="relative mb-6">
        <Search className="absolute left-3 top-1/2 -translate-y-1/2 h-4 w-4 text-gray-400" />
        <input
          type="text"
          placeholder="Search movies..."
          value={search}
          onChange={(e) => { setSearch(e.target.value); setPage(1); }}
          className="w-full pl-10 pr-4 py-2 border border-gray-300 dark:border-gray-600 rounded-lg bg-white dark:bg-gray-800 text-gray-900 dark:text-white"
        />
      </div>

      {/* Table */}
      <div className="bg-white dark:bg-gray-800 rounded-xl shadow overflow-hidden">
        <div className="overflow-x-auto">
          <table className="w-full text-left">
            <thead className="bg-gray-50 dark:bg-gray-700">
              <tr>
                <th className="px-4 py-3 text-xs font-medium text-gray-500 dark:text-gray-400 uppercase">Movie</th>
                <th className="px-4 py-3 text-xs font-medium text-gray-500 dark:text-gray-400 uppercase">Language</th>
                <th className="px-4 py-3 text-xs font-medium text-gray-500 dark:text-gray-400 uppercase">Certificate</th>
                <th className="px-4 py-3 text-xs font-medium text-gray-500 dark:text-gray-400 uppercase">Release</th>
                <th className="px-4 py-3 text-xs font-medium text-gray-500 dark:text-gray-400 uppercase">Rating</th>
                <th className="px-4 py-3 text-xs font-medium text-gray-500 dark:text-gray-400 uppercase">Actions</th>
              </tr>
            </thead>
            <tbody className="divide-y divide-gray-200 dark:divide-gray-700">
              {isLoading ? (
                <tr><td colSpan={6} className="px-4 py-8 text-center text-gray-500">Loading...</td></tr>
              ) : !movies?.items?.length ? (
                <tr><td colSpan={6} className="px-4 py-8 text-center text-gray-500">No movies found</td></tr>
              ) : (
                movies.items.map((m) => (
                  <tr key={m.id} className="hover:bg-gray-50 dark:hover:bg-gray-750">
                    <td className="px-4 py-3">
                      <div className="flex items-center gap-3">
                        {m.posterUrl && (
                          <img src={m.posterUrl} alt="" className="w-10 h-14 object-cover rounded" />
                        )}
                        <div>
                          <p className="font-medium text-gray-900 dark:text-white text-sm">{m.title}</p>
                          <p className="text-xs text-gray-500">{m.genres.join(', ')}</p>
                        </div>
                      </div>
                    </td>
                    <td className="px-4 py-3 text-sm text-gray-700 dark:text-gray-300">{m.language}</td>
                    <td className="px-4 py-3 text-sm text-gray-700 dark:text-gray-300">{m.certificate}</td>
                    <td className="px-4 py-3 text-sm text-gray-700 dark:text-gray-300">
                      {new Date(m.releaseDate).toLocaleDateString('en-IN', { day: 'numeric', month: 'short', year: 'numeric' })}
                    </td>
                    <td className="px-4 py-3 text-sm">
                      {m.averageRating > 0 ? (
                        <span className="text-yellow-500">★ {m.averageRating.toFixed(1)}</span>
                      ) : (
                        <span className="text-gray-400">—</span>
                      )}
                    </td>
                    <td className="px-4 py-3">
                      <div className="flex items-center gap-2">
                        <button
                          onClick={() => openEdit(m.id)}
                          className="p-1.5 text-blue-600 hover:bg-blue-50 dark:hover:bg-blue-900/20 rounded"
                          title="Edit"
                        >
                          <Pencil className="h-4 w-4" />
                        </button>
                        <button
                          onClick={() => handleDelete(m.id, m.title)}
                          className="p-1.5 text-red-600 hover:bg-red-50 dark:hover:bg-red-900/20 rounded"
                          title="Delete"
                        >
                          <Trash2 className="h-4 w-4" />
                        </button>
                      </div>
                    </td>
                  </tr>
                ))
              )}
            </tbody>
          </table>
        </div>

        {/* Pagination */}
        {movies && movies.totalPages > 1 && (
          <div className="flex items-center justify-between px-4 py-3 border-t border-gray-200 dark:border-gray-700">
            <p className="text-sm text-gray-500">
              Page {movies.page} of {movies.totalPages} ({movies.totalCount} movies)
            </p>
            <div className="flex gap-2">
              <button
                onClick={() => setPage((p) => Math.max(1, p - 1))}
                disabled={page === 1}
                className="px-3 py-1 text-sm border rounded disabled:opacity-50 dark:border-gray-600 dark:text-gray-300"
              >
                Prev
              </button>
              <button
                onClick={() => setPage((p) => p + 1)}
                disabled={page >= movies.totalPages}
                className="px-3 py-1 text-sm border rounded disabled:opacity-50 dark:border-gray-600 dark:text-gray-300"
              >
                Next
              </button>
            </div>
          </div>
        )}
      </div>

      {/* Modal Form */}
      {showForm && (
        <div className="fixed inset-0 z-50 flex items-center justify-center bg-black/50 p-4">
          <div className="bg-white dark:bg-gray-800 rounded-xl shadow-xl w-full max-w-2xl max-h-[90vh] overflow-y-auto">
            <div className="flex items-center justify-between px-6 py-4 border-b dark:border-gray-700">
              <h2 className="text-lg font-bold text-gray-900 dark:text-white">
                {editingId ? 'Edit Movie' : 'Add Movie'}
              </h2>
              <button onClick={() => setShowForm(false)} className="text-gray-400 hover:text-gray-600">
                <X className="h-5 w-5" />
              </button>
            </div>

            <form onSubmit={handleSubmit} className="p-6 space-y-4">
              {/* Title */}
              <div>
                <label className="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-1">Title *</label>
                <input
                  type="text"
                  value={form.title}
                  onChange={(e) => setForm({ ...form, title: e.target.value })}
                  className="w-full px-3 py-2 border rounded-lg dark:border-gray-600 dark:bg-gray-700 dark:text-white"
                  required
                />
              </div>

              {/* Description */}
              <div>
                <label className="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-1">Description *</label>
                <textarea
                  value={form.description}
                  onChange={(e) => setForm({ ...form, description: e.target.value })}
                  rows={3}
                  className="w-full px-3 py-2 border rounded-lg dark:border-gray-600 dark:bg-gray-700 dark:text-white"
                  required
                />
              </div>

              {/* Row: Duration, Language, Certificate */}
              <div className="grid grid-cols-3 gap-4">
                <div>
                  <label className="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-1">Duration (min)</label>
                  <input
                    type="number"
                    value={form.durationMinutes}
                    onChange={(e) => setForm({ ...form, durationMinutes: parseInt(e.target.value) || 0 })}
                    className="w-full px-3 py-2 border rounded-lg dark:border-gray-600 dark:bg-gray-700 dark:text-white"
                    min={1}
                  />
                </div>
                <div>
                  <label className="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-1">Language</label>
                  <input
                    type="text"
                    value={form.language}
                    onChange={(e) => setForm({ ...form, language: e.target.value })}
                    className="w-full px-3 py-2 border rounded-lg dark:border-gray-600 dark:bg-gray-700 dark:text-white"
                  />
                </div>
                <div>
                  <label className="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-1">Certificate</label>
                  <select
                    value={form.certificate}
                    onChange={(e) => setForm({ ...form, certificate: parseInt(e.target.value) })}
                    className="w-full px-3 py-2 border rounded-lg dark:border-gray-600 dark:bg-gray-700 dark:text-white"
                  >
                    {CERTIFICATES.map((c) => (
                      <option key={c.value} value={c.value}>{c.label}</option>
                    ))}
                  </select>
                </div>
              </div>

              {/* Release Date */}
              <div>
                <label className="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-1">Release Date</label>
                <input
                  type="date"
                  value={form.releaseDate}
                  onChange={(e) => setForm({ ...form, releaseDate: e.target.value })}
                  className="w-full px-3 py-2 border rounded-lg dark:border-gray-600 dark:bg-gray-700 dark:text-white"
                />
              </div>

              {/* Poster & Trailer URLs */}
              <div className="grid grid-cols-2 gap-4">
                <div>
                  <label className="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-1">Poster URL</label>
                  <input
                    type="url"
                    value={form.posterUrl}
                    onChange={(e) => setForm({ ...form, posterUrl: e.target.value })}
                    className="w-full px-3 py-2 border rounded-lg dark:border-gray-600 dark:bg-gray-700 dark:text-white"
                    placeholder="https://..."
                  />
                </div>
                <div>
                  <label className="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-1">Trailer URL</label>
                  <input
                    type="url"
                    value={form.trailerUrl}
                    onChange={(e) => setForm({ ...form, trailerUrl: e.target.value })}
                    className="w-full px-3 py-2 border rounded-lg dark:border-gray-600 dark:bg-gray-700 dark:text-white"
                    placeholder="https://..."
                  />
                </div>
              </div>

              {/* Genres */}
              <div>
                <label className="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-2">Genres</label>
                <div className="flex flex-wrap gap-2">
                  {genres.map((g) => (
                    <button
                      key={g.id}
                      type="button"
                      onClick={() => toggleGenre(g.id)}
                      className={`px-3 py-1 text-sm rounded-full border transition-colors ${
                        form.genreIds.includes(g.id)
                          ? 'bg-primary-600 text-white border-primary-600'
                          : 'border-gray-300 dark:border-gray-600 text-gray-700 dark:text-gray-300 hover:border-primary-400'
                      }`}
                    >
                      {g.name}
                    </button>
                  ))}
                </div>
              </div>

              {/* Active toggle (edit only) */}
              {editingId && (
                <div className="flex items-center gap-2">
                  <input
                    type="checkbox"
                    id="isActive"
                    checked={form.isActive}
                    onChange={(e) => setForm({ ...form, isActive: e.target.checked })}
                    className="rounded"
                  />
                  <label htmlFor="isActive" className="text-sm text-gray-700 dark:text-gray-300">Active (visible to users)</label>
                </div>
              )}

              {/* Actions */}
              <div className="flex justify-end gap-3 pt-4 border-t dark:border-gray-700">
                <button
                  type="button"
                  onClick={() => setShowForm(false)}
                  className="px-4 py-2 text-sm border rounded-lg dark:border-gray-600 dark:text-gray-300 hover:bg-gray-50 dark:hover:bg-gray-700"
                >
                  Cancel
                </button>
                <button
                  type="submit"
                  disabled={saving}
                  className="btn-primary px-6 py-2 disabled:opacity-50"
                >
                  {saving ? 'Saving...' : editingId ? 'Update Movie' : 'Create Movie'}
                </button>
              </div>
            </form>
          </div>
        </div>
      )}
    </div>
  );
}
