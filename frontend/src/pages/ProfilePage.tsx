import { useState, useEffect } from 'react';
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { usersApi } from '@/api';
import { useAuth } from '@/store/AuthContext';
import { PageLoader } from '@/components/Loading';
import { User, Lock, Save, Phone, Mail, Calendar, Ticket, Star } from 'lucide-react';
import { formatDate } from '@/lib/utils';
import toast from 'react-hot-toast';

export function ProfilePage() {
  const { logout } = useAuth();
  const queryClient = useQueryClient();

  const { data, isLoading } = useQuery({
    queryKey: ['profile'],
    queryFn: () => usersApi.getProfile(),
  });

  const [firstName, setFirstName] = useState('');
  const [lastName, setLastName] = useState('');
  const [phone, setPhone] = useState('');
  const [currentPassword, setCurrentPassword] = useState('');
  const [newPassword, setNewPassword] = useState('');
  const [confirmPassword, setConfirmPassword] = useState('');

  const profile = data?.data;

  useEffect(() => {
    if (profile) {
      setFirstName(profile.firstName);
      setLastName(profile.lastName);
      setPhone(profile.phone || '');
    }
  }, [profile]);

  const updateProfileMutation = useMutation({
    mutationFn: () => usersApi.updateProfile({ firstName, lastName, phone: phone || undefined }),
    onSuccess: () => {
      toast.success('Profile updated successfully');
      queryClient.invalidateQueries({ queryKey: ['profile'] });
      // Update localStorage user data
      const storedUser = localStorage.getItem('user');
      if (storedUser) {
        const userData = JSON.parse(storedUser);
        userData.firstName = firstName;
        userData.lastName = lastName;
        userData.phone = phone || null;
        localStorage.setItem('user', JSON.stringify(userData));
      }
    },
    onError: (err: any) => {
      toast.error(err.response?.data?.error || 'Failed to update profile');
    },
  });

  const changePasswordMutation = useMutation({
    mutationFn: () => usersApi.changePassword({ currentPassword, newPassword }),
    onSuccess: () => {
      toast.success('Password changed successfully. Please login again.');
      setCurrentPassword('');
      setNewPassword('');
      setConfirmPassword('');
      setTimeout(() => logout(), 2000);
    },
    onError: (err: any) => {
      toast.error(err.response?.data?.error || 'Failed to change password');
    },
  });

  const handleUpdateProfile = (e: React.FormEvent) => {
    e.preventDefault();
    if (!firstName.trim() || !lastName.trim()) {
      toast.error('First name and last name are required');
      return;
    }
    updateProfileMutation.mutate();
  };

  const passwordRules = [
    { test: (p: string) => p.length >= 8, msg: 'At least 8 characters' },
    { test: (p: string) => /[A-Z]/.test(p), msg: 'One uppercase letter' },
    { test: (p: string) => /[a-z]/.test(p), msg: 'One lowercase letter' },
    { test: (p: string) => /[0-9]/.test(p), msg: 'One digit' },
    { test: (p: string) => /[^a-zA-Z0-9]/.test(p), msg: 'One special character' },
  ];

  const handleChangePassword = (e: React.FormEvent) => {
    e.preventDefault();
    const failed = passwordRules.find((r) => !r.test(newPassword));
    if (failed) {
      toast.error(`New password: ${failed.msg}`);
      return;
    }
    if (newPassword !== confirmPassword) {
      toast.error('Passwords do not match');
      return;
    }
    changePasswordMutation.mutate();
  };

  if (isLoading) return <PageLoader />;
  if (!profile) return <div className="text-center py-20 text-gray-600 dark:text-gray-400">Profile not found</div>;

  return (
    <div className="max-w-3xl mx-auto px-4 py-8">
      <h1 className="text-3xl font-bold mb-8">My Profile</h1>

      {/* Stats Cards */}
      <div className="grid grid-cols-2 md:grid-cols-4 gap-4 mb-8">
        <div className="card p-4 text-center">
          <User className="h-6 w-6 text-primary-500 mx-auto mb-1" />
          <p className="text-xs text-gray-600 dark:text-gray-400">Role</p>
          <p className="font-semibold">{profile.role}</p>
        </div>
        <div className="card p-4 text-center">
          <Ticket className="h-6 w-6 text-green-500 mx-auto mb-1" />
          <p className="text-xs text-gray-600 dark:text-gray-400">Bookings</p>
          <p className="font-semibold">{profile.totalBookings}</p>
        </div>
        <div className="card p-4 text-center">
          <Star className="h-6 w-6 text-yellow-500 mx-auto mb-1" />
          <p className="text-xs text-gray-600 dark:text-gray-400">Reviews</p>
          <p className="font-semibold">{profile.totalReviews}</p>
        </div>
        <div className="card p-4 text-center">
          <Calendar className="h-6 w-6 text-blue-500 mx-auto mb-1" />
          <p className="text-xs text-gray-600 dark:text-gray-400">Joined</p>
          <p className="font-semibold text-sm">{formatDate(profile.createdAt)}</p>
        </div>
      </div>

      {/* Profile Info */}
      <div className="card p-6 mb-6">
        <h2 className="text-lg font-semibold mb-4 flex items-center gap-2">
          <User className="h-5 w-5 text-primary-500" />
          Personal Information
        </h2>
        <form onSubmit={handleUpdateProfile} className="space-y-4">
          <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
            <div>
              <label className="block text-sm text-gray-600 dark:text-gray-400 mb-1">First Name</label>
              <input
                type="text"
                value={firstName}
                onChange={(e) => setFirstName(e.target.value)}
                className="input-field"
                required
              />
            </div>
            <div>
              <label className="block text-sm text-gray-600 dark:text-gray-400 mb-1">Last Name</label>
              <input
                type="text"
                value={lastName}
                onChange={(e) => setLastName(e.target.value)}
                className="input-field"
                required
              />
            </div>
          </div>
          <div>
            <label className="block text-sm text-gray-600 dark:text-gray-400 mb-1">
              <Mail className="inline h-4 w-4 mr-1" />Email
            </label>
            <input type="email" value={profile.email} disabled className="input-field opacity-60 cursor-not-allowed" />
          </div>
          <div>
            <label className="block text-sm text-gray-600 dark:text-gray-400 mb-1">
              <Phone className="inline h-4 w-4 mr-1" />Phone
            </label>
            <input
              type="tel"
              value={phone}
              onChange={(e) => setPhone(e.target.value)}
              className="input-field"
              placeholder="+91 9876543210"
            />
          </div>
          <button
            type="submit"
            disabled={updateProfileMutation.isPending}
            className="btn-primary flex items-center gap-2"
          >
            <Save className="h-4 w-4" />
            {updateProfileMutation.isPending ? 'Saving...' : 'Save Changes'}
          </button>
        </form>
      </div>

      {/* Change Password */}
      <div className="card p-6">
        <h2 className="text-lg font-semibold mb-4 flex items-center gap-2">
          <Lock className="h-5 w-5 text-primary-500" />
          Change Password
        </h2>
        <form onSubmit={handleChangePassword} className="space-y-4">
          <div>
            <label className="block text-sm text-gray-600 dark:text-gray-400 mb-1">Current Password</label>
            <input
              type="password"
              value={currentPassword}
              onChange={(e) => setCurrentPassword(e.target.value)}
              className="input-field"
              required
            />
          </div>
          <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
            <div>
              <label className="block text-sm text-gray-600 dark:text-gray-400 mb-1">New Password</label>
              <input
                type="password"
                value={newPassword}
                onChange={(e) => setNewPassword(e.target.value)}
                className="input-field"
                required
              />
              {newPassword && (
                <ul className="mt-1 space-y-0.5">
                  {passwordRules.map((r) => (
                    <li key={r.msg} className={`text-xs flex items-center gap-1 ${r.test(newPassword) ? 'text-green-400' : 'text-gray-500'}`}>
                      <span>{r.test(newPassword) ? '✓' : '·'}</span> {r.msg}
                    </li>
                  ))}
                </ul>
              )}
            </div>
            <div>
              <label className="block text-sm text-gray-600 dark:text-gray-400 mb-1">Confirm New Password</label>
              <input
                type="password"
                value={confirmPassword}
                onChange={(e) => setConfirmPassword(e.target.value)}
                className="input-field"
                required
              />
            </div>
          </div>
          <button
            type="submit"
            disabled={changePasswordMutation.isPending}
            className="btn-primary flex items-center gap-2"
          >
            <Lock className="h-4 w-4" />
            {changePasswordMutation.isPending ? 'Changing...' : 'Change Password'}
          </button>
        </form>
      </div>
    </div>
  );
}
