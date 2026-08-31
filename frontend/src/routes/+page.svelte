<script lang="ts">
  import { onMount } from 'svelte';
  import { api } from '$lib/api/client';
  import { authState } from '$lib/state/auth.svelte';
  import { Search, Briefcase, Users, FileText, ArrowRight } from '@lucide/svelte';

  let positions = $state<any[]>([]);
  let stats = $state<any>(null);
  let loading = $state(true);

  onMount(async () => {
    try {
      // Fetch public positions (latest 5 active)
      const posResponse = await api.get<any>('/positions?page=1&pageSize=5');
      positions = posResponse.items || [];

      // If recruiter, fetch dashboard stats
      if (authState.isRecruiter || authState.isAdmin) {
        stats = await api.get<any>('/statistics');
      }
    } catch (e) {
      console.error(e);
    } finally {
      loading = false;
    }
  });
</script>

<svelte:head>
  <title>Home - CV Nexus</title>
</svelte:head>

<div class="space-y-12 pb-12">
  <!-- Hero Section -->
  <section class="relative rounded-3xl overflow-hidden border border-slate-700/50 bg-slate-900/40 backdrop-blur-md shadow-2xl p-8 md:p-12 lg:p-16 flex flex-col items-center text-center">
    <!-- Abstract BG Elements inside Hero -->
    <div class="absolute top-0 right-1/4 w-64 h-64 bg-indigo-500/20 rounded-full blur-3xl -z-10"></div>
    <div class="absolute bottom-0 left-1/4 w-64 h-64 bg-purple-500/20 rounded-full blur-3xl -z-10"></div>
    
    <div class="inline-flex items-center gap-2 px-3 py-1 rounded-full bg-slate-800/50 border border-slate-700 text-sm font-medium text-indigo-300 mb-6">
      <span class="relative flex h-2 w-2">
        <span class="animate-ping absolute inline-flex h-full w-full rounded-full bg-indigo-400 opacity-75"></span>
        <span class="relative inline-flex rounded-full h-2 w-2 bg-indigo-500"></span>
      </span>
      Platform v1.0 is live
    </div>
    
    <h1 class="text-4xl md:text-5xl lg:text-6xl font-black tracking-tight mb-6">
      The Next Generation <br class="hidden sm:block" />
      <span class="bg-clip-text text-transparent bg-gradient-to-r from-indigo-400 via-purple-400 to-pink-400">
        Recruitment Platform
      </span>
    </h1>
    
    <p class="max-w-2xl text-lg text-slate-400 mb-10 leading-relaxed">
      A dynamic, attribute-driven CV management system. Stop relying on static PDFs and start using structured data to find the perfect match.
    </p>
    
    <div class="flex flex-col sm:flex-row gap-4 w-full max-w-md justify-center">
      {#if !authState.isAuthenticated}
        <a href="/register" class="px-8 py-3.5 rounded-xl bg-gradient-to-r from-indigo-600 to-purple-600 text-white font-bold shadow-lg shadow-indigo-500/25 hover:from-indigo-500 hover:to-purple-500 transition-all hover:-translate-y-1">
          Get Started
        </a>
        <a href="/positions" class="px-8 py-3.5 rounded-xl bg-slate-800 border border-slate-700 text-slate-200 font-bold hover:bg-slate-700 transition-all">
          Browse Positions
        </a>
      {:else if authState.isCandidate}
        <a href="/positions" class="px-8 py-3.5 rounded-xl bg-gradient-to-r from-indigo-600 to-purple-600 text-white font-bold shadow-lg shadow-indigo-500/25 hover:from-indigo-500 hover:to-purple-500 transition-all hover:-translate-y-1">
          Explore Positions
        </a>
        <a href="/profile" class="px-8 py-3.5 rounded-xl bg-slate-800 border border-slate-700 text-slate-200 font-bold hover:bg-slate-700 transition-all flex items-center justify-center gap-2">
          My Profile <ArrowRight size={18} />
        </a>
      {:else}
        <a href="/positions/new" class="px-8 py-3.5 rounded-xl bg-gradient-to-r from-indigo-600 to-purple-600 text-white font-bold shadow-lg shadow-indigo-500/25 hover:from-indigo-500 hover:to-purple-500 transition-all hover:-translate-y-1 flex items-center justify-center gap-2">
          Create Position <Briefcase size={18} />
        </a>
      {/if}
    </div>
  </section>

  <!-- Recruiter Dashboard Stats -->
  {#if stats}
    <div class="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-4 gap-6">
      <div class="bg-slate-900/60 backdrop-blur-sm border border-slate-700/50 rounded-2xl p-6 flex items-center gap-4 hover:bg-slate-800/80 transition-colors shadow-lg">
        <div class="p-3 bg-indigo-500/20 text-indigo-400 rounded-xl">
          <Users size={24} />
        </div>
        <div>
          <p class="text-sm font-medium text-slate-400">Total Candidates</p>
          <p class="text-2xl font-black text-white">{stats.totalCandidates}</p>
        </div>
      </div>
      
      <div class="bg-slate-900/60 backdrop-blur-sm border border-slate-700/50 rounded-2xl p-6 flex items-center gap-4 hover:bg-slate-800/80 transition-colors shadow-lg">
        <div class="p-3 bg-purple-500/20 text-purple-400 rounded-xl">
          <Briefcase size={24} />
        </div>
        <div>
          <p class="text-sm font-medium text-slate-400">Active Positions</p>
          <p class="text-2xl font-black text-white">{stats.totalActivePositions}</p>
        </div>
      </div>
      
      <div class="bg-slate-900/60 backdrop-blur-sm border border-slate-700/50 rounded-2xl p-6 flex items-center gap-4 hover:bg-slate-800/80 transition-colors shadow-lg">
        <div class="p-3 bg-pink-500/20 text-pink-400 rounded-xl">
          <FileText size={24} />
        </div>
        <div>
          <p class="text-sm font-medium text-slate-400">CVs Submitted</p>
          <p class="text-2xl font-black text-white">{stats.totalCVsSubmitted}</p>
        </div>
      </div>
      
      <div class="bg-slate-900/60 backdrop-blur-sm border border-slate-700/50 rounded-2xl p-6 flex items-center gap-4 hover:bg-slate-800/80 transition-colors shadow-lg">
        <div class="p-3 bg-blue-500/20 text-blue-400 rounded-xl">
          <Search size={24} />
        </div>
        <div>
          <p class="text-sm font-medium text-slate-400">Discussions</p>
          <p class="text-2xl font-black text-white">{stats.totalDiscussions}</p>
        </div>
      </div>
    </div>
  {/if}

  <!-- Latest Positions -->
  <section class="space-y-6">
    <div class="flex items-center justify-between">
      <h2 class="text-2xl font-bold">Latest Opportunities</h2>
      <a href="/positions" class="text-sm font-medium text-indigo-400 hover:text-indigo-300 flex items-center gap-1 group">
        View all <ArrowRight size={16} class="group-hover:translate-x-1 transition-transform" />
      </a>
    </div>

    {#if loading}
      <div class="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-6 animate-pulse">
        {#each [1, 2, 3] as _}
          <div class="h-48 bg-slate-800/50 rounded-2xl border border-slate-700/50"></div>
        {/each}
      </div>
    {:else if positions.length > 0}
      <div class="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-6">
        {#each positions as pos}
          <a href={`/positions/${pos.id}`} class="group block bg-slate-900/50 backdrop-blur-sm border border-slate-700/50 hover:border-indigo-500/50 rounded-2xl p-6 transition-all hover:shadow-lg hover:shadow-indigo-500/10 hover:-translate-y-1">
            <div class="flex justify-between items-start mb-4">
              <div class="p-2.5 bg-slate-800 rounded-xl text-slate-300 group-hover:bg-indigo-500 group-hover:text-white transition-colors">
                <Briefcase size={20} />
              </div>
              <span class="text-xs font-medium px-2.5 py-1 bg-green-500/10 text-green-400 rounded-full border border-green-500/20">
                {pos.level || 'Mid'}
              </span>
            </div>
            <h3 class="text-xl font-bold text-slate-100 mb-2 truncate group-hover:text-indigo-300 transition-colors">{pos.title}</h3>
            {#if pos.company}
              <p class="text-sm text-slate-400 font-medium mb-4 flex items-center gap-1.5">
                <span class="w-1.5 h-1.5 rounded-full bg-slate-500"></span> {pos.company}
              </p>
            {/if}
            <p class="text-slate-400 text-sm line-clamp-2 leading-relaxed mb-6">
              {pos.shortDescription || 'No description provided.'}
            </p>
            <div class="flex items-center text-sm font-semibold text-indigo-400">
              Apply Now <ArrowRight size={16} class="ml-1 opacity-0 -translate-x-2 group-hover:opacity-100 group-hover:translate-x-0 transition-all" />
            </div>
          </a>
        {/each}
      </div>
    {:else}
      <div class="text-center py-12 bg-slate-900/30 rounded-2xl border border-slate-800 border-dashed">
        <Briefcase class="mx-auto h-12 w-12 text-slate-600 mb-3" />
        <h3 class="text-lg font-medium text-slate-300">No open positions right now</h3>
        <p class="text-slate-500 mt-1">Check back later for new opportunities.</p>
      </div>
    {/if}
  </section>
</div>
