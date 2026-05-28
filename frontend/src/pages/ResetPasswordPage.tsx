import { useState } from 'react';
import { Link, useSearchParams } from 'react-router-dom';
import { usersApi } from '@/api';
import { Lock, CheckCircle } from 'lucide-react';
import toast from 'react-hot-toast';

export function ResetPasswordPage() {
  const [searchParams] = useSearchParams();
  const token = searchParams.get('token') || '';
  const [newPassword, setNewPassword] = useState('');
  const [confirmPassword, setConfirmPassword] = useState('');
  const [isSubmitting, setIsSubmitting] = useState(false);
  const [isReset, setIsReset] = useState(false);

  const passwordRules = [
    { test: (p: string) => p.length >= 8, msg: 'At least 8 characters' },
    { test: (p: string) => /[A-Z]/.test(p), msg: 'One uppercase letter' },
    { test: (p: string) => /[a-z]/.test(p), msg: 'One lowercase letter' },
    { test: (p: string) => /[0-9]/.test(p), msg: 'One digit' },
    { test: (p: string) => /[^a-zA-Z0-9]/.test(p), msg: 'One special character' },
  ];

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    const failed = passwordRules.find((r) => !r.test(newPassword));
    if (failed) {
      toast.error(`Password: ${failed.msg}`);
      return;
    }
    if (newPassword !== confirmPassword) {
      toast.error('Passwords do not match');
      return;
    }

    setIsSubmitting(true);
    try {
      await usersApi.resetPassword({ token, newPassword });
      setIsReset(true);
    } catch (err: any) {
      toast.error(err.response?.data?.error || 'Invalid or expired reset token');
    } finally {
      setIsSubmitting(false);
    }
  };

  if (!token) {
    return (
      <div className="max-w-md mx-auto px-4 py-16 text-center">
        <h1 className="text-2xl font-bold mb-4 text-red-400">Invalid Reset Link</h1>
        <p className="text-gray-600 dark:text-gray-400 mb-6">
          This password reset link is invalid or has expired.
        </p>
        <Link to="/forgot-password" className="btn-primary">Request New Link</Link>
      </div>
    );
  }

  if (isReset) {
    return (
      <div className="max-w-md mx-auto px-4 py-16 text-center">
        <CheckCircle className="h-16 w-16 text-green-400 mx-auto mb-4" />
        <h1 className="text-2xl font-bold mb-2">Password Reset Successfully</h1>
        <p className="text-gray-600 dark:text-gray-400 mb-6">
          You can now login with your new password.
        </p>
        <Link to="/login" className="btn-primary">Go to Login</Link>
      </div>
    );
  }

  return (
    <div className="max-w-md mx-auto px-4 py-16">
      <h1 className="text-2xl font-bold mb-2">Reset Password</h1>
      <p className="text-gray-600 dark:text-gray-400 mb-6">Enter your new password below.</p>

      <form onSubmit={handleSubmit} className="card p-6 space-y-4">
        <div>
          <label className="block text-sm text-gray-600 dark:text-gray-400 mb-1">
            <Lock className="inline h-4 w-4 mr-1" />New Password
          </label>
          <input
            type="password"
            value={newPassword}
            onChange={(e) => setNewPassword(e.target.value)}
            className="input-field"
            placeholder="Enter new password"
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
          <label className="block text-sm text-gray-600 dark:text-gray-400 mb-1">
            <Lock className="inline h-4 w-4 mr-1" />Confirm Password
          </label>
          <input
            type="password"
            value={confirmPassword}
            onChange={(e) => setConfirmPassword(e.target.value)}
            className="input-field"
            placeholder="Confirm new password"
            required
          />
        </div>
        <button type="submit" disabled={isSubmitting} className="btn-primary w-full">
          {isSubmitting ? 'Resetting...' : 'Reset Password'}
        </button>
      </form>
    </div>
  );
}
