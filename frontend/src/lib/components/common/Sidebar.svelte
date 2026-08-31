<script lang="ts">
  import { authState } from '$lib/state/auth.svelte';
  import { 
    LayoutDashboard, 
    Briefcase, 
    FileText, 
    User as UserIcon, 
    Tags, 
    Settings,
    Search
  } from '@lucide/svelte';
  import { page } from '$app/stores';

  let { isOpen, closeSidebar } = $props<{ isOpen: boolean; closeSidebar: () => void }>();

  // Use a derived state to determine the current path for active link styling
  const currentPath = $derived($page.url.pathname);
  
  const navItems = $derived.by(() => {
    const items = [];
    
    // Public routes
    items.push({ href: '/', icon: LayoutDashboard, label: 'Dashboard' });
    items.push({ href: '/search', icon: Search, label: 'Global Search' });
    items.push({ href: '/positions', icon: Briefcase, label: 'Open Positions' });

    // Authenticated routes
    if (authState.isAuthenticated) {
      if (authState.isCandidate) {
        items.push({ href: '/profile', icon: UserIcon, label: 'My Profile' });
        items.push({ href: '/cvs', icon: FileText, label: 'My Applications' });
      }
      
      if (authState.isRecruiter || authState.isAdmin) {
        items.push({ href: '/attributes', icon: Tags, label: 'Attribute Library' });
      }
      
      items.push({ href: '/settings', icon: Settings, label: 'Settings' });
    }
    
    return items;
  });
</script>

<!-- Mobile Overlay -->
{#if isOpen}
  <div 
    class="fixed inset-0 z-40 bg-slate-950/80 backdrop-blur-sm md:hidden transition-opacity" 
    onclick={closeSidebar}
    onkeydown={(e) => e.key === 'Escape' && closeSidebar()}
    role="button"
    tabindex="0"
    aria-label="Close sidebar"
  ></div>
{/if}

<!-- Sidebar -->
<aside 
  class="fixed inset-y-0 left-0 z-50 w-64 bg-slate-900 border-r border-slate-700/50 transform transition-transform duration-300 ease-in-out md:translate-x-0 md:static md:block shadow-xl shadow-slate-900/50 flex flex-col {isOpen ? 'translate-x-0' : '-translate-x-full'}"
>
  <div class="h-16 flex items-center px-6 border-b border-slate-700/50 md:hidden">
    <span class="text-xl font-bold bg-clip-text text-transparent bg-gradient-to-r from-indigo-400 to-purple-400">CV Nexus</span>
  </div>

  <div class="flex-1 overflow-y-auto py-6 px-3">
    <nav class="space-y-1.5">
      {#each navItems as item}
        <a 
          href={item.href}
          class="flex items-center gap-3 px-3 py-2.5 rounded-lg text-sm font-medium transition-all duration-200 {currentPath === item.href || (item.href !== '/' && currentPath.startsWith(item.href)) ? 'bg-indigo-500/15 text-indigo-300 border border-indigo-500/30' : 'text-slate-400 hover:text-slate-100 hover:bg-white/5'}"
          onclick={() => {
            if (window.innerWidth < 768) closeSidebar();
          }}
        >
          <item.icon size={18} class={currentPath === item.href || (item.href !== '/' && currentPath.startsWith(item.href)) ? 'text-indigo-400' : ''} />
          {item.label}
        </a>
      {/each}
    </nav>
  </div>
  
  {#if authState.isAuthenticated}
    <div class="p-4 border-t border-slate-700/50 bg-slate-900/50">
      <div class="flex items-center gap-3">
        <div class="h-10 w-10 rounded-full bg-gradient-to-tr from-indigo-500 to-purple-500 flex items-center justify-center text-white font-bold shadow-md shadow-indigo-500/20">
          {authState.user?.username?.charAt(0).toUpperCase()}
        </div>
        <div class="flex flex-col overflow-hidden">
          <span class="text-sm font-semibold text-slate-200 truncate">{authState.user?.username}</span>
          <span class="text-xs text-slate-400 truncate">{authState.user?.email}</span>
        </div>
      </div>
    </div>
  {/if}
</aside>
