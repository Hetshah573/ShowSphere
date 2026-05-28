import { useState } from 'react';
import { useForm } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';
import { z } from 'zod';
import { Link, useNavigate } from 'react-router-dom';
import { useAuth } from '@/store/AuthContext';
import { Mail, Lock, User, Phone, Eye, EyeOff } from 'lucide-react';
import toast from 'react-hot-toast';

const registerSchema = z.object({
  firstName: z.string().min(1, 'First name is required').max(100),
  lastName: z.string().min(1, 'Last name is required').max(100),
  email: z.string().email('Invalid email address'),
  phone: z.string().optional(),
  password: z
    .string()
    .min(8, 'Password must be at least 8 characters')
    .regex(/[A-Z]/, 'Must contain uppercase letter')
    .regex(/[a-z]/, 'Must contain lowercase letter')
    .regex(/[0-9]/, 'Must contain a digit')
    .regex(/[^a-zA-Z0-9]/, 'Must contain a special character'),
  confirmPassword: z.string(),
}).refine((data) => data.password === data.confirmPassword, {
  message: 'Passwords do not match',
  path: ['confirmPassword'],
});

type RegisterFormData = z.infer<typeof registerSchema>;

export function RegisterPage() {
  const { register: authRegister } = useAuth();
  const navigate = useNavigate();
  const [showPassword, setShowPassword] = useState(false);
  const [isSubmitting, setIsSubmitting] = useState(false);

  const { register, handleSubmit, watch, formState: { errors } } = useForm<RegisterFormData>({
    resolver: zodResolver(registerSchema),
  });

  const passwordValue = watch('password', '');

  const passwordRules = [
    { test: (p: string) => p.length >= 8, msg: 'At least 8 characters' },
    { test: (p: string) => /[A-Z]/.test(p), msg: 'One uppercase letter' },
    { test: (p: string) => /[a-z]/.test(p), msg: 'One lowercase letter' },
    { test: (p: string) => /[0-9]/.test(p), msg: 'One digit' },
    { test: (p: string) => /[^a-zA-Z0-9]/.test(p), msg: 'One special character' },
  ];

  const onSubmit = async (data: RegisterFormData) => {
    setIsSubmitting(true);
    try {
      await authRegister(data.email, data.password, data.firstName, data.lastName, data.phone);
      navigate('/');
    } catch (err: unknown) {
      const error = err as { response?: { data?: { error?: string } } };
      toast.error(error.response?.data?.error || 'Registration failed');
    } finally {
      setIsSubmitting(false);
    }
  };

  return (
    <div className="min-h-[80vh] flex items-center justify-center px-4 py-8">
      <div className="w-full max-w-md">
        <div className="card p-8">
          <h1 className="text-2xl font-bold text-center mb-2">Create Account</h1>
          <p className="text-gray-400 text-center mb-8">Join ShowSphere today</p>

          <form onSubmit={handleSubmit(onSubmit)} className="space-y-4">
            <div className="grid grid-cols-2 gap-3">
              <div>
                <label className="block text-sm font-medium text-gray-300 mb-1">First Name</label>
                <div className="relative">
                  <User className="absolute left-3 top-3 h-4 w-4 text-gray-500" />
                  <input {...register('firstName')} className="input-field pl-9 text-sm" placeholder="John" />
                </div>
                {errors.firstName && <p className="text-red-400 text-xs mt-1">{errors.firstName.message}</p>}
              </div>
              <div>
                <label className="block text-sm font-medium text-gray-300 mb-1">Last Name</label>
                <input {...register('lastName')} className="input-field text-sm" placeholder="Doe" />
                {errors.lastName && <p className="text-red-400 text-xs mt-1">{errors.lastName.message}</p>}
              </div>
            </div>

            <div>
              <label className="block text-sm font-medium text-gray-300 mb-1">Email</label>
              <div className="relative">
                <Mail className="absolute left-3 top-3 h-4 w-4 text-gray-500" />
                <input type="email" {...register('email')} className="input-field pl-9" placeholder="you@example.com" />
              </div>
              {errors.email && <p className="text-red-400 text-xs mt-1">{errors.email.message}</p>}
            </div>

            <div>
              <label className="block text-sm font-medium text-gray-300 mb-1">Phone (optional)</label>
              <div className="relative">
                <Phone className="absolute left-3 top-3 h-4 w-4 text-gray-500" />
                <input {...register('phone')} className="input-field pl-9" placeholder="+91 9876543210" />
              </div>
            </div>

            <div>
              <label className="block text-sm font-medium text-gray-300 mb-1">Password</label>
              <div className="relative">
                <Lock className="absolute left-3 top-3 h-4 w-4 text-gray-500" />
                <input
                  type={showPassword ? 'text' : 'password'}
                  {...register('password')}
                  className="input-field pl-9 pr-10"
                  placeholder="••••••••"
                />
                <button type="button" onClick={() => setShowPassword(!showPassword)} className="absolute right-3 top-3 text-gray-500">
                  {showPassword ? <EyeOff className="h-4 w-4" /> : <Eye className="h-4 w-4" />}
                </button>
              </div>
              {passwordValue && (
                <ul className="mt-1 space-y-0.5">
                  {passwordRules.map((r) => (
                    <li key={r.msg} className={`text-xs flex items-center gap-1 ${r.test(passwordValue) ? 'text-green-400' : 'text-gray-500'}`}>
                      <span>{r.test(passwordValue) ? '✓' : '·'}</span> {r.msg}
                    </li>
                  ))}
                </ul>
              )}
              {errors.password && !passwordValue && <p className="text-red-400 text-xs mt-1">{errors.password.message}</p>}
            </div>

            <div>
              <label className="block text-sm font-medium text-gray-300 mb-1">Confirm Password</label>
              <input type="password" {...register('confirmPassword')} className="input-field" placeholder="••••••••" />
              {errors.confirmPassword && <p className="text-red-400 text-xs mt-1">{errors.confirmPassword.message}</p>}
            </div>

            <button type="submit" disabled={isSubmitting} className="btn-primary w-full">
              {isSubmitting ? 'Creating Account...' : 'Create Account'}
            </button>
          </form>

          <p className="text-center text-gray-400 mt-6">
            Already have an account?{' '}
            <Link to="/login" className="text-primary-400 hover:text-primary-300">Sign In</Link>
          </p>
        </div>
      </div>
    </div>
  );
}
