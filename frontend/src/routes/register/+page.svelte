<script lang="ts">
  import { api } from '$lib/api/client';
  import { authState } from '$lib/state/auth.svelte';
  import { goto } from '$app/navigation';
  import { Loader2 } from '@lucide/svelte';

  let username = $state('');
  let email = $state('');
  let password = $state('');
  let role = $state('Candidate'); // default role
  let loading = $state(false);
  let errorMsg = $state('');

  async function handleRegister(e: Event) {
    e.preventDefault();
    loading = true;
    errorMsg = '';
    
    try {
      // 1. Register the user
      await api.post('/auth/register', { username, email, password, role });
      
      // 2. Automatically login after successful registration
      const loginResponse = await api.post<{ accessToken: string }>('/auth/login', { email, password });
      authState.setToken(loginResponse.accessToken);
      
      // 3. Navigate home
      goto('/');
    } catch (err: any) {
      errorMsg = err.message || 'Failed to register';
    } finally {
      loading = false;
    }
  }

  function handleGoogleLogin() {
    window.location.href = `${import.meta.env.VITE_API_URL || 'http://localhost:5246/api'}/auth/google-login`;
  }
</script>

<svelte:head>
  <title>Register - CV Nexus</title>
</svelte:head>

<div class="flex items-center justify-center min-h-[80vh]">
  <div class="w-full max-w-md p-8 rounded-2xl backdrop-blur-xl bg-slate-900/60 border border-slate-700/50 shadow-2xl">
    <div class="text-center mb-8">
      <h1 class="text-3xl font-black bg-clip-text text-transparent bg-gradient-to-r from-indigo-400 to-purple-400">Join CV Nexus</h1>
      <p class="text-slate-400 mt-2">Create an account to start applying or recruiting</p>
    </div>

    {#if errorMsg}
      <div class="mb-6 p-4 rounded-lg bg-red-500/10 border border-red-500/20 text-red-400 text-sm">
        {errorMsg}
      </div>
    {/if}

    <form onsubmit={handleRegister} class="space-y-4">
      <div>
        <label for="username" class="block text-sm font-medium text-slate-300 mb-1.5">Username</label>
        <input 
          id="username" 
          type="text" 
          bind:value={username} 
          required 
          class="w-full px-4 py-2.5 rounded-lg bg-slate-950 border border-slate-700 text-white placeholder-slate-500 focus:border-indigo-500 focus:ring-1 focus:ring-indigo-500 transition-colors"
          placeholder="johndoe"
        />
      </div>

      <div>
        <label for="email" class="block text-sm font-medium text-slate-300 mb-1.5">Email</label>
        <input 
          id="email" 
          type="email" 
          bind:value={email} 
          required 
          class="w-full px-4 py-2.5 rounded-lg bg-slate-950 border border-slate-700 text-white placeholder-slate-500 focus:border-indigo-500 focus:ring-1 focus:ring-indigo-500 transition-colors"
          placeholder="you@example.com"
        />
      </div>

      <div>
        <label for="password" class="block text-sm font-medium text-slate-300 mb-1.5">Password</label>
        <input 
          id="password" 
          type="password" 
          bind:value={password} 
          required 
          class="w-full px-4 py-2.5 rounded-lg bg-slate-950 border border-slate-700 text-white placeholder-slate-500 focus:border-indigo-500 focus:ring-1 focus:ring-indigo-500 transition-colors"
          placeholder="••••••••"
        />
      </div>
      
      <div>
        <label for="role" class="block text-sm font-medium text-slate-300 mb-1.5">I am a...</label>
        <select 
          id="role" 
          bind:value={role} 
          class="w-full px-4 py-2.5 rounded-lg bg-slate-950 border border-slate-700 text-white focus:border-indigo-500 focus:ring-1 focus:ring-indigo-500 transition-colors"
        >
          <option value="Candidate">Candidate looking for jobs</option>
          <option value="Recruiter">Recruiter posting positions</option>
        </select>
      </div>

      <button 
        type="submit" 
        disabled={loading}
        class="w-full py-2.5 mt-2 rounded-lg bg-gradient-to-r from-indigo-600 to-purple-600 text-white font-semibold shadow-lg shadow-indigo-500/25 hover:from-indigo-500 hover:to-purple-500 focus:ring-2 focus:ring-indigo-500 focus:ring-offset-2 focus:ring-offset-slate-900 transition-all disabled:opacity-70 flex justify-center items-center"
      >
        {#if loading}
          <Loader2 class="animate-spin mr-2" size={20} />
          Creating account...
        {:else}
          Sign Up
        {/if}
      </button>
    </form>

    <div class="mt-6 relative">
      <div class="absolute inset-0 flex items-center">
        <div class="w-full border-t border-slate-700/50"></div>
      </div>
      <div class="relative flex justify-center text-sm">
        <span class="px-4 bg-slate-900/60 text-slate-400">Or continue with</span>
      </div>
    </div>

    <div class="mt-6">
      <button 
        onclick={handleGoogleLogin}
        class="w-full py-2.5 rounded-lg bg-white text-slate-900 font-semibold hover:bg-slate-100 transition-colors flex items-center justify-center gap-3"
      >
        <svg viewBox="0 0 24 24" class="w-5 h-5"><path d="M22.56 12.25c0-.78-.07-1.53-.2-2.25H12v4.26h5.92c-.26 1.37-1.04 2.53-2.21 3.31v2.77h3.57c2.08-1.92 3.28-4.74 3.28-8.09z" fill="#4285F4"/><path d="M12 23c2.97 0 5.46-.98 7.28-2.66l-3.57-2.77c-.98.66-2.23 1.06-3.71 1.06-2.86 0-5.29-1.93-6.16-4.53H2.18v2.84C3.99 20.53 7.7 23 12 23z" fill="#34A853"/><path d="M5.84 14.09c-.22-.66-.35-1.36-.35-2.09s.13-1.43.35-2.09V7.07H2.18C1.43 8.55 1 10.22 1 12s.43 3.45 1.18 4.93l2.85-2.22.81-.62z" fill="#FBBC05"/><path d="M12 5.38c1.62 0 3.06.56 4.21 1.64l3.15-3.15C17.45 2.09 14.97 1 12 1 7.7 1 3.99 3.47 2.18 7.07l3.66 2.84c.87-2.6 3.3-4.53 6.16-4.53z" fill="#EA4335"/></svg>
        Google
      </button>
    </div>

    <p class="mt-8 text-center text-sm text-slate-400">
      Already have an account? 
      <a href="/login" class="font-semibold text-indigo-400 hover:text-indigo-300 transition-colors">Sign in</a>
    </p>
  </div>
</div>
