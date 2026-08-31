<script lang="ts">
  import { onMount } from 'svelte';
  import { page } from '$app/stores';
  import { api } from '$lib/api/client';
  import { authState } from '$lib/state/auth.svelte';
  import { goto } from '$app/navigation';
  import { Loader2, Briefcase, Calendar, Tag, CheckCircle2, ArrowRight } from '@lucide/svelte';

  let positionId = $page.params.id;
  let position = $state<any>(null);
  let loading = $state(true);
  let errorMsg = $state('');
  let applying = $state(false);

  onMount(async () => {
    try {
      position = await api.get<any>(`/positions/${positionId}`);
    } catch (e: any) {
      errorMsg = e.message || 'Failed to load position';
    } finally {
      loading = false;
    }
  });

  async function handleApply() {
    if (!authState.isAuthenticated) {
      goto('/login');
      return;
    }
    
    applying = true;
    errorMsg = '';
    
    try {
      // Create a draft CV
      const cv = await api.post<any>('/cvs', { positionId });
      // Navigate to the CV editor
      goto(`/cvs/${cv.id}`);
    } catch (e: any) {
      errorMsg = e.message || 'Failed to start application. You may not meet the access rules.';
    } finally {
      applying = false;
    }
  }
</script>

<svelte:head>
  <title>{position?.title || 'Position'} - CV Nexus</title>
</svelte:head>

<div class="max-w-4xl mx-auto pb-12">
  {#if loading}
    <div class="flex items-center justify-center py-20">
      <Loader2 class="animate-spin text-indigo-500" size={40} />
    </div>
  {:else if errorMsg && !position}
    <div class="p-4 rounded-lg bg-red-500/10 border border-red-500/20 text-red-400 text-sm mb-6">
      {errorMsg}
    </div>
    <a href="/positions" class="text-indigo-400 hover:text-indigo-300">← Back to positions</a>
  {:else if position}
    <div class="mb-6">
      <a href="/positions" class="text-slate-400 hover:text-white transition-colors text-sm flex items-center gap-1">
        ← Back to positions
      </a>
    </div>
    
    <div class="bg-slate-900/40 backdrop-blur-md border border-slate-700/50 rounded-3xl overflow-hidden shadow-2xl">
      <!-- Header -->
      <div class="p-8 md:p-10 border-b border-slate-700/50 relative overflow-hidden">
        <div class="absolute top-0 right-0 w-64 h-64 bg-indigo-500/10 rounded-full blur-3xl -z-10 pointer-events-none"></div>
        
        <div class="flex flex-col md:flex-row md:items-start justify-between gap-6">
          <div>
            <div class="flex items-center gap-3 mb-4">
              <span class="text-xs font-medium px-3 py-1 bg-indigo-500/10 text-indigo-400 rounded-full border border-indigo-500/20">
                {position.level || 'Mid Level'}
              </span>
              {#if !position.isActive}
                <span class="text-xs font-medium px-3 py-1 bg-red-500/10 text-red-400 rounded-full border border-red-500/20">
                  Closed
                </span>
              {/if}
            </div>
            
            <h1 class="text-3xl md:text-4xl font-bold text-white mb-2">{position.title}</h1>
            {#if position.company}
              <p class="text-xl text-slate-300 flex items-center gap-2">
                <Briefcase size={20} class="text-slate-500" /> {position.company}
              </p>
            {/if}
          </div>
          
          <div class="flex flex-col gap-3 shrink-0 w-full md:w-auto">
            {#if authState.isRecruiter || authState.isAdmin}
              <a href={`/positions/${position.id}/edit`} class="w-full text-center px-6 py-3 rounded-xl bg-slate-800 text-white font-semibold hover:bg-slate-700 transition-colors border border-slate-700">
                Edit Position
              </a>
              <a href={`/positions/${position.id}/cvs`} class="w-full text-center px-6 py-3 rounded-xl bg-indigo-600 text-white font-semibold hover:bg-indigo-500 transition-colors">
                View Applicants
              </a>
            {:else if position.isActive}
              <button 
                onclick={handleApply}
                disabled={applying}
                class="w-full px-8 py-3.5 rounded-xl bg-gradient-to-r from-indigo-600 to-purple-600 text-white font-bold shadow-lg shadow-indigo-500/25 hover:from-indigo-500 hover:to-purple-500 transition-all hover:-translate-y-1 disabled:opacity-70 disabled:hover:translate-y-0 disabled:cursor-not-allowed flex items-center justify-center gap-2"
              >
                {#if applying}
                  <Loader2 class="animate-spin" size={20} /> Starting...
                {:else}
                  Apply Now <ArrowRight size={18} />
                {/if}
              </button>
            {/if}
          </div>
        </div>
      </div>
      
      <!-- Body -->
      <div class="p-8 md:p-10 space-y-10">
        {#if errorMsg && position}
          <div class="p-4 rounded-lg bg-red-500/10 border border-red-500/20 text-red-400 text-sm">
            {errorMsg}
          </div>
        {/if}

        <section>
          <h3 class="text-xl font-bold text-white mb-4">About this position</h3>
          <div class="prose prose-invert prose-indigo max-w-none text-slate-300">
            {position.shortDescription || 'No description provided.'}
          </div>
        </section>

        <!-- Dynamic Requirements -->
        <section>
          <h3 class="text-xl font-bold text-white mb-6 border-b border-slate-700/50 pb-2">Requirements</h3>
          
          <div class="bg-slate-900/50 rounded-2xl border border-slate-700 p-6">
            <h4 class="font-medium text-indigo-400 mb-4 flex items-center gap-2">
              <CheckCircle2 size={18} /> Required Attributes
            </h4>
            {#if position.requiredAttributes && position.requiredAttributes.length > 0}
              <ul class="space-y-3">
                {#each position.requiredAttributes as attr}
                  <li class="flex items-start gap-3">
                    <span class="w-1.5 h-1.5 rounded-full bg-slate-500 mt-2"></span>
                    <div>
                      <span class="font-medium text-slate-200">{attr.attributeDefinition.name}</span>
                      <span class="text-slate-400 text-sm ml-2">({attr.attributeDefinition.category})</span>
                    </div>
                  </li>
                {/each}
              </ul>
            {:else}
              <p class="text-slate-500 text-sm italic">No specific attributes required.</p>
            {/if}
          </div>
        </section>

        <!-- Project Tags -->
        {#if position.projectTags && position.projectTags.length > 0}
          <section>
            <h3 class="text-xl font-bold text-white mb-4">Relevant Project Tags</h3>
            <p class="text-slate-400 text-sm mb-4">Your application will auto-filter your projects to highlight these technologies.</p>
            <div class="flex flex-wrap gap-2">
              {#each position.projectTags as tag}
                <span class="px-3 py-1.5 rounded-lg bg-slate-800 border border-slate-700 text-slate-300 text-sm font-medium flex items-center gap-1.5">
                  <Tag size={14} class="text-slate-500" /> {tag.tag}
                </span>
              {/each}
            </div>
          </section>
        {/if}
      </div>
    </div>
  {/if}
</div>
