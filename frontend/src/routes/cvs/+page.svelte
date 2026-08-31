<script lang="ts">
  import { onMount } from 'svelte';
  import { api } from '$lib/api/client';
  import { authState } from '$lib/state/auth.svelte';
  import { Loader2, FileText, Briefcase, Calendar, ExternalLink, Trash2 } from '@lucide/svelte';

  let cvs = $state<any[]>([]);
  let loading = $state(true);
  let errorMsg = $state('');

  onMount(async () => {
    if (!authState.isAuthenticated || !authState.isCandidate) {
      window.location.href = '/login';
      return;
    }

    try {
      const data = await api.get<any>('/cvs/me?page=1&pageSize=50');
      cvs = data.items || [];
    } catch (e: any) {
      errorMsg = e.message || 'Failed to load CVs';
    } finally {
      loading = false;
    }
  });

  function getStatusColor(status: string) {
    return status === 'Published' 
      ? 'bg-green-500/10 text-green-400 border-green-500/20' 
      : 'bg-yellow-500/10 text-yellow-400 border-yellow-500/20';
  }
</script>

<svelte:head>
  <title>My Applications - CV Nexus</title>
</svelte:head>

<div class="max-w-5xl mx-auto pb-12">
  <div class="flex items-center justify-between mb-8">
    <div>
      <h1 class="text-3xl font-bold text-slate-100">My Applications</h1>
      <p class="text-slate-400 mt-1">Manage your drafts and submitted CVs.</p>
    </div>
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
  {:else if cvs.length === 0}
    <div class="text-center py-20 bg-slate-900/30 rounded-2xl border border-slate-800 border-dashed">
      <FileText class="mx-auto h-16 w-16 text-slate-600 mb-4" />
      <h3 class="text-xl font-medium text-slate-300">No applications yet</h3>
      <p class="text-slate-500 mt-2 mb-6">You haven't started any applications.</p>
      <a href="/positions" class="px-6 py-2.5 rounded-lg bg-indigo-600 text-white font-medium hover:bg-indigo-500 transition-colors">
        Browse Positions
      </a>
    </div>
  {:else}
    <div class="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-6">
      {#each cvs as cv}
        <div class="group flex flex-col bg-slate-900/40 backdrop-blur-md border border-slate-700/50 hover:border-slate-600 rounded-2xl p-6 transition-all hover:shadow-lg">
          <div class="flex justify-between items-start mb-4">
            <div class="flex items-center gap-2">
              <span class="text-xs font-semibold px-2.5 py-1 rounded-full border {getStatusColor(cv.status)}">
                {cv.status}
              </span>
            </div>
            <button class="text-slate-500 hover:text-red-400 p-1.5 rounded-lg hover:bg-red-400/10 transition-colors" title="Delete">
              <Trash2 size={16} />
            </button>
          </div>
          
          <h3 class="text-lg font-bold text-slate-100 mb-1">{cv.positionTitle}</h3>
          
          <div class="space-y-2 mt-4 text-sm text-slate-400 flex-1">
            <div class="flex items-center gap-2">
              <Briefcase size={14} class="text-slate-500" />
              <span>{cv.company || 'Unknown Company'}</span>
            </div>
            <div class="flex items-center gap-2">
              <Calendar size={14} class="text-slate-500" />
              <span>Last updated: {new Date(cv.updatedAt).toLocaleDateString()}</span>
            </div>
          </div>
          
          <div class="mt-6 pt-4 border-t border-slate-800">
            <a href={`/cvs/${cv.id}`} class="w-full flex items-center justify-center gap-2 py-2.5 rounded-xl bg-slate-800 text-white font-medium hover:bg-slate-700 transition-colors border border-slate-700 hover:border-slate-600 group-hover:bg-indigo-600 group-hover:border-indigo-500 group-hover:text-white">
              {cv.status === 'Draft' ? 'Continue Editing' : 'View Submitted CV'} <ExternalLink size={16} />
            </a>
          </div>
        </div>
      {/each}
    </div>
  {/if}
</div>
