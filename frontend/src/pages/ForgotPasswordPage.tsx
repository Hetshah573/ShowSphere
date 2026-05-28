import { useState } from 'react';
import { Link } from 'react-router-dom';
import { usersApi } from '@/api';
import { Mail, ArrowLeft, CheckCircle } from 'lucide-react';
import toast from 'react-hot-toast';

export function ForgotPasswordPage() {
  const [email, setEmail] = useState('');
  const [isSubmitting, setIsSubmitting] = useState(false);
  const [isSent, setIsSent] = useState(false);

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    if (!email.trim()) return;

    setIsSubmitting(true);
    try {
      await usersApi.forgotPassword(email);
      setIsSent(true);
    } catch {
      toast.error('Something went wrong. Please try again.');
    } finally {
      setIsSubmitting(false);
    }
  };

  if (isSent) {
    return (
      <div className="max-w-md mx-auto px-4 py-16 text-center">
        <CheckCircle className="h-16 w-16 text-green-400 mx-auto mb-4" />
        <h1 className="text-2xl font-bold mb-2">Check Your Email</h1>
        <p className="text-gray-600 dark:text-gray-400 mb-6">
          If an account exists with <strong>{email}</strong>, we've sent a password reset link.
        </p>
        <p className="text-sm text-gray-500 dark:text-gray-400 mb-6">
          (In this demo, the reset token is logged to the server console)
        </p>
        <Link to="/login" className="btn-primary inline-flex items-center gap-2">
          <ArrowLeft className="h-4 w-4" /> Back to Login
        </Link>
      </div>
    );
  }

  return (
    <div className="max-w-md mx-auto px-4 py-16">
      <h1 className="text-2xl font-bold mb-2">Forgot Password</h1>
      <p className="text-gray-600 dark:text-gray-400 mb-6">
        Enter your email address and we'll send you a link to reset your password.
      </p>

      <form onSubmit={handleSubmit} className="card p-6 space-y-4">
        <div>
          <label className="block text-sm text-gray-600 dark:text-gray-400 mb-1">
            <Mail className="inline h-4 w-4 mr-1" />Email Address
          </label>
          <input
            type="email"
            value={email}
            onChange={(e) => setEmail(e.target.value)}
            className="input-field"
            placeholder="Enter your email"
            required
          />
        </div>
        <button type="submit" disabled={isSubmitting} className="btn-primary w-full">
          {isSubmitting ? 'Sending...' : 'Send Reset Link'}
        </button>
        <Link to="/login" className="block text-center text-sm text-primary-600 dark:text-primary-400 hover:underline">
          Back to Login
        </Link>
      </form>
    </div>
  );
}
