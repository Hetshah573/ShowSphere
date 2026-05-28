import { Link, useNavigate } from 'react-router-dom';
import { useAuth } from '@/store/AuthContext';
import { useTheme } from '@/store/ThemeContext';
import { Film, User, LogOut, Ticket, LayoutDashboard, Menu, X, Sun, Moon, Heart } from 'lucide-react';
import { useState } from 'react';

export function Navbar() {
  const { user, isAuthenticated, isAdmin, logout } = useAuth();
  const { theme, toggleTheme } = useTheme();
  const navigate = useNavigate();
  const [mobileOpen, setMobileOpen] = useState(false);

  const handleLogout = () => {
    logout();
    navigate('/');
  };

  return (
    <nav className="bg-white/95 dark:bg-gray-800/95 backdrop-blur-sm border-b border-gray-200 dark:border-gray-700 sticky top-0 z-50">
      <div className="max-w-7xl mx-auto px-4">
        <div className="flex items-center justify-between h-16">
          {/* Logo */}
          <Link to="/" className="flex items-center gap-2 text-xl font-bold text-primary-600 dark:text-primary-400">
            <Film className="h-7 w-7" />
            <span>ShowSphere</span>
          </Link>

          {/* Desktop Nav */}
          <div className="hidden md:flex items-center gap-6">
            <Link to="/movies" className="text-gray-600 dark:text-gray-300 hover:text-gray-900 dark:hover:text-white transition-colors">
              Movies
            </Link>
            {isAuthenticated && (
              <Link to="/bookings" className="text-gray-600 dark:text-gray-300 hover:text-gray-900 dark:hover:text-white transition-colors flex items-center gap-1">
                <Ticket className="h-4 w-4" /> My Bookings
              </Link>
            )}
            {isAuthenticated && (
              <Link to="/wishlist" className="text-gray-600 dark:text-gray-300 hover:text-gray-900 dark:hover:text-white transition-colors flex items-center gap-1">
                <Heart className="h-4 w-4" /> Wishlist
              </Link>
            )}
            {isAdmin && (
              <Link to="/admin" className="text-gray-600 dark:text-gray-300 hover:text-gray-900 dark:hover:text-white transition-colors flex items-center gap-1">
                <LayoutDashboard className="h-4 w-4" /> Admin
              </Link>
            )}
            {isAdmin && (
              <Link to="/admin/movies" className="text-gray-600 dark:text-gray-300 hover:text-gray-900 dark:hover:text-white transition-colors flex items-center gap-1">
                Manage Movies
              </Link>
            )}
            {isAdmin && (
              <Link to="/admin/shows" className="text-gray-600 dark:text-gray-300 hover:text-gray-900 dark:hover:text-white transition-colors flex items-center gap-1">
                Manage Shows
              </Link>
            )}
          </div>

          {/* Auth Actions & Theme Toggle */}
          <div className="hidden md:flex items-center gap-3">
            <button
              onClick={toggleTheme}
              className="p-2 rounded-lg text-gray-500 dark:text-gray-400 hover:bg-gray-100 dark:hover:bg-gray-700 transition-colors"
              aria-label="Toggle theme"
            >
              {theme === 'dark' ? <Sun className="h-5 w-5" /> : <Moon className="h-5 w-5" />}
            </button>
            {isAuthenticated ? (
              <div className="flex items-center gap-3">
                <Link to="/profile" className="text-sm text-gray-600 dark:text-gray-300 flex items-center gap-1 hover:text-primary-500 transition-colors">
                  <User className="h-4 w-4" /> {user?.firstName}
                </Link>
                <button onClick={handleLogout} className="text-gray-400 hover:text-red-400 transition-colors">
                  <LogOut className="h-5 w-5" />
                </button>
              </div>
            ) : (
              <div className="flex items-center gap-2">
                <Link to="/login" className="btn-secondary text-sm py-2 px-4">
                  Sign In
                </Link>
                <Link to="/register" className="btn-primary text-sm py-2 px-4">
                  Sign Up
                </Link>
              </div>
            )}
          </div>

          {/* Mobile Toggle */}
          <div className="md:hidden flex items-center gap-2">
            <button
              onClick={toggleTheme}
              className="p-2 rounded-lg text-gray-500 dark:text-gray-400 hover:bg-gray-100 dark:hover:bg-gray-700 transition-colors"
              aria-label="Toggle theme"
            >
              {theme === 'dark' ? <Sun className="h-5 w-5" /> : <Moon className="h-5 w-5" />}
            </button>
            <button className="text-gray-600 dark:text-gray-300" onClick={() => setMobileOpen(!mobileOpen)}>
              {mobileOpen ? <X className="h-6 w-6" /> : <Menu className="h-6 w-6" />}
            </button>
          </div>
        </div>

        {/* Mobile Menu */}
        {mobileOpen && (
          <div className="md:hidden py-4 border-t border-gray-200 dark:border-gray-700 space-y-3">
            <Link to="/movies" className="block text-gray-600 dark:text-gray-300 hover:text-gray-900 dark:hover:text-white" onClick={() => setMobileOpen(false)}>
              Movies
            </Link>
            {isAuthenticated && (
              <Link to="/bookings" className="block text-gray-600 dark:text-gray-300 hover:text-gray-900 dark:hover:text-white" onClick={() => setMobileOpen(false)}>
                My Bookings
              </Link>
            )}
            {isAuthenticated && (
              <Link to="/wishlist" className="block text-gray-600 dark:text-gray-300 hover:text-gray-900 dark:hover:text-white" onClick={() => setMobileOpen(false)}>
                Wishlist
              </Link>
            )}
            {isAuthenticated && (
              <Link to="/profile" className="block text-gray-600 dark:text-gray-300 hover:text-gray-900 dark:hover:text-white" onClick={() => setMobileOpen(false)}>
                Profile
              </Link>
            )}
            {isAdmin && (
              <Link to="/admin" className="block text-gray-600 dark:text-gray-300 hover:text-gray-900 dark:hover:text-white" onClick={() => setMobileOpen(false)}>
                Admin Dashboard
              </Link>
            )}
            {isAdmin && (
              <Link to="/admin/movies" className="block text-gray-600 dark:text-gray-300 hover:text-gray-900 dark:hover:text-white" onClick={() => setMobileOpen(false)}>
                Manage Movies
              </Link>
            )}
            {isAdmin && (
              <Link to="/admin/shows" className="block text-gray-600 dark:text-gray-300 hover:text-gray-900 dark:hover:text-white" onClick={() => setMobileOpen(false)}>
                Manage Shows
              </Link>
            )}
            {isAuthenticated ? (
              <button onClick={handleLogout} className="text-red-400">
                Logout
              </button>
            ) : (
              <div className="flex gap-2">
                <Link to="/login" className="btn-secondary text-sm" onClick={() => setMobileOpen(false)}>Sign In</Link>
                <Link to="/register" className="btn-primary text-sm" onClick={() => setMobileOpen(false)}>Sign Up</Link>
              </div>
            )}
          </div>
        )}
      </div>
    </nav>
  );
}
