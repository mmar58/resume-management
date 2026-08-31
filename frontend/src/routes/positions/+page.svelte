<script lang="ts">
  import { onMount } from 'svelte';
  import { api } from '$lib/api/client';
  import { authState } from '$lib/state/auth.svelte';
  import { Loader2, Briefcase, Search, Plus, Filter, ArrowRight } from '@lucide/svelte';

  let positions = $state<any[]>([]);
  let loading = $state(true);
  let errorMsg = $state('');
  
  let searchQuery = $state('');
  let page = $state(1);
  let totalCount = $state(0);
  let pageSize = 12;

  async function loadPositions() {
    loading = true;
    errorMsg = '';
    
    try {
      // If we had a real search endpoint we'd use it here, 
      // but for now we just use the paginated /positions endpoint.
      // (In a real app, /positions might take a search query param)
      const data = await api.get<any>(`/positions?page=${page}&pageSize=${pageSize}`);
      positions = data.items || [];
      totalCount = data.totalCount || 0;
    } catch (e: any) {
      errorMsg = e.message || 'Failed to load positions';
    } finally {
      loading = false;
    }
  }

  onMount(() => {
    loadPositions();
  });
</script>

<svelte:head>
  <title>Open Positions - CV Nexus</title>
</svelte:head>

<div class="max-w-6xl mx-auto">
  <div class="flex flex-col sm:flex-row sm:items-center justify-between gap-4 mb-8">
    <div>
      <h1 class="text-3xl font-bold text-slate-100">Open Positions</h1>
      <p class="text-slate-400 mt-1">Discover opportunities that match your skills.</p>
    </div>
    
    {#if authState.isRecruiter || authState.isAdmin}
      <a href="/positions/new" class="inline-flex items-center gap-2 px-4 py-2.5 rounded-lg bg-indigo-600 text-white font-medium hover:bg-indigo-500 transition-colors shadow-lg shadow-indigo-500/20">
        <Plus size={18} /> Create Position
      </a>
    {/if}
  </div>

  <div class="flex gap-4 mb-8">
    <div class="relative flex-1">
      <div class="absolute inset-y-0 left-0 pl-3 flex items-center pointer-events-none">
        <Search size={18} class="text-slate-500" />
      </div>
      <input 
        type="text" 
        bind:value={searchQuery}
        placeholder="Search positions by title, company, or keywords..." 
        class="w-full pl-10 pr-4 py-3 rounded-xl bg-slate-900/60 border border-slate-700 text-white placeholder-slate-500 focus:border-indigo-500 focus:ring-1 focus:ring-indigo-500 transition-colors backdrop-blur-sm"
      />
    </div>
    <button class="px-4 py-3 rounded-xl bg-slate-900/60 border border-slate-700 text-slate-300 hover:bg-slate-800 transition-colors flex items-center gap-2 backdrop-blur-sm">
      <Filter size={18} /> <span class="hidden sm:inline">Filters</span>
    </button>
  </div>

  {#if errorMsg}
    <div class="mb-8 p-4 rounded-lg bg-red-500/10 border border-red-500/20 text-red-400 text-sm">
      {errorMsg}
    </div>
  {/if}

  {#if loading}
    <div class="flex items-center justify-center py-20">
      <Loader2 class="animate-spin text-indigo-500" size={40} />
    </div>
  {:else if positions.length === 0}
    <div class="text-center py-20 bg-slate-900/30 rounded-2xl border border-slate-800 border-dashed">
      <Briefcase class="mx-auto h-16 w-16 text-slate-600 mb-4" />
      <h3 class="text-xl font-medium text-slate-300">No positions found</h3>
      <p class="text-slate-500 mt-2">Try adjusting your filters or search query.</p>
    </div>
  {:else}
    <div class="grid grid-cols-1 md:grid-cols-2 xl:grid-cols-3 gap-6">
      {#each positions as pos}
        <a href={`/positions/${pos.id}`} class="group flex flex-col bg-slate-900/40 backdrop-blur-md border border-slate-700/50 hover:border-indigo-500/50 rounded-2xl p-6 transition-all hover:shadow-lg hover:shadow-indigo-500/10 hover:-translate-y-1">
          <div class="flex justify-between items-start mb-4">
            <div class="p-3 bg-slate-800/80 rounded-xl text-slate-300 group-hover:bg-indigo-500 group-hover:text-white transition-colors">
              <Briefcase size={24} />
            </div>
            <div class="flex items-center gap-2">
              {#if !pos.isActive}
                <span class="text-xs font-medium px-2.5 py-1 bg-red-500/10 text-red-400 rounded-full border border-red-500/20">
                  Closed
                </span>
              {/if}
              <span class="text-xs font-medium px-2.5 py-1 bg-slate-800 text-slate-300 rounded-full border border-slate-700">
                {pos.level || 'Mid'}
              </span>
            </div>
          </div>
          
          <h3 class="text-xl font-bold text-slate-100 mb-1 group-hover:text-indigo-300 transition-colors">{pos.title}</h3>
          
          {#if pos.company}
            <p class="text-sm text-slate-400 font-medium mb-4 flex items-center gap-1.5">
              <span class="w-1.5 h-1.5 rounded-full bg-slate-500"></span> {pos.company}
            </p>
          {/if}
          
          <p class="text-slate-400 text-sm line-clamp-3 leading-relaxed mb-6 flex-1">
            {pos.shortDescription || 'No description provided.'}
          </p>
          
          <div class="mt-auto pt-4 border-t border-slate-800 flex items-center justify-between text-sm font-semibold text-indigo-400">
            <span>View Details</span>
            <ArrowRight size={16} class="opacity-0 -translate-x-2 group-hover:opacity-100 group-hover:translate-x-0 transition-all" />
          </div>
        </a>
      {/each}
    </div>
    
    <!-- Basic Pagination -->
    {#if totalCount > pageSize}
      <div class="mt-12 flex justify-center gap-2">
        <button 
          disabled={page === 1}
          onclick={() => { page--; loadPositions(); }}
          class="px-4 py-2 rounded-lg bg-slate-800 text-slate-300 hover:bg-slate-700 disabled:opacity-50 disabled:cursor-not-allowed transition-colors"
        >
          Previous
        </button>
        <span class="px-4 py-2 text-slate-400">Page {page} of {Math.ceil(totalCount / pageSize)}</span>
        <button 
          disabled={page >= Math.ceil(totalCount / pageSize)}
          onclick={() => { page++; loadPositions(); }}
          class="px-4 py-2 rounded-lg bg-slate-800 text-slate-300 hover:bg-slate-700 disabled:opacity-50 disabled:cursor-not-allowed transition-colors"
        >
          Next
        </button>
      </div>
    {/if}
  {/if}
</div>
