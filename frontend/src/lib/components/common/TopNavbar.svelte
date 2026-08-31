<script lang="ts">
  import { authState } from '$lib/state/auth.svelte';
  import { LogOut, User, Menu, Bell } from '@lucide/svelte';
  
  let { toggleSidebar } = $props<{ toggleSidebar: () => void }>();
</script>

<header class="sticky top-0 z-40 w-full backdrop-blur-md bg-slate-900/80 border-b border-slate-700/50 shadow-sm">
  <div class="container flex h-16 items-center justify-between px-4 sm:px-8">
    <div class="flex items-center gap-4">
      <button 
        onclick={toggleSidebar} 
        class="text-slate-300 hover:text-white transition-colors md:hidden p-2 -ml-2 rounded-full hover:bg-white/10"
      >
        <Menu size={24} />
      </button>
      <a href="/" class="flex items-center gap-2">
        <div class="bg-gradient-to-br from-indigo-500 to-purple-600 text-white font-black p-1.5 rounded-lg">
          <svg xmlns="http://www.w3.org/2000/svg" width="24" height="24" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"><path d="M14 2H6a2 2 0 0 0-2 2v16a2 2 0 0 0 2 2h12a2 2 0 0 0 2-2V8z"></path><polyline points="14 2 14 8 20 8"></polyline><line x1="16" y1="13" x2="8" y2="13"></line><line x1="16" y1="17" x2="8" y2="17"></line><polyline points="10 9 9 9 8 9"></polyline></svg>
        </div>
        <span class="text-xl font-bold bg-clip-text text-transparent bg-gradient-to-r from-indigo-400 to-purple-400 tracking-tight hidden sm:inline-block">
          CV Nexus
        </span>
      </a>
    </div>

    <div class="flex items-center gap-4">
      <button class="text-slate-400 hover:text-white p-2 rounded-full hover:bg-white/10 transition-colors">
        <Bell size={20} />
      </button>
      
      {#if authState.isAuthenticated}
        <div class="flex items-center gap-3 pl-4 border-l border-slate-700/50">
          <div class="flex flex-col items-end hidden sm:flex">
            <span class="text-sm font-semibold text-slate-200">{authState.user?.username}</span>
            <span class="text-xs text-slate-400">{authState.isRecruiter ? 'Recruiter' : authState.isAdmin ? 'Admin' : 'Candidate'}</span>
          </div>
          <button 
            class="h-9 w-9 rounded-full bg-gradient-to-tr from-indigo-500 to-purple-500 flex items-center justify-center text-white font-bold shadow-lg shadow-indigo-500/20"
          >
            {authState.user?.username?.charAt(0).toUpperCase()}
          </button>
          <button 
            onclick={() => authState.logout()}
            class="text-slate-400 hover:text-red-400 p-2 rounded-full hover:bg-white/10 transition-colors ml-1"
            title="Log out"
          >
            <LogOut size={20} />
          </button>
        </div>
      {:else}
        <div class="flex items-center gap-2">
          <a href="/login" class="text-sm font-medium text-slate-300 hover:text-white px-4 py-2 rounded-md hover:bg-white/5 transition-colors">
            Log in
          </a>
          <a href="/register" class="text-sm font-medium bg-indigo-500 hover:bg-indigo-600 text-white px-4 py-2 rounded-md shadow-lg shadow-indigo-500/20 transition-all">
            Sign up
          </a>
        </div>
      {/if}
    </div>
  </div>
</header>
