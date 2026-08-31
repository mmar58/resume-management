<script lang="ts">
  import { onMount } from 'svelte';
  import { api } from '$lib/api/client';
  import { authState } from '$lib/state/auth.svelte';
  import { Loader2, User, FileText, Settings, Briefcase, Plus } from '@lucide/svelte';

  let profile = $state<any>(null);
  let loading = $state(true);
  let saving = $state(false);
  let errorMsg = $state('');
  let successMsg = $state('');

  // Built-in fields
  let firstName = $state('');
  let lastName = $state('');
  let phone = $state('');
  let location = $state('');
  let photoUrl = $state('');

  // Selected tab
  let activeTab = $state('general'); // general, attributes, projects

  onMount(async () => {
    if (!authState.isAuthenticated || !authState.isCandidate) {
      window.location.href = '/login';
      return;
    }

    try {
      const data = await api.get<any>('/profile');
      profile = data;
      firstName = data.firstName || '';
      lastName = data.lastName || '';
      phone = data.phone || '';
      location = data.location || '';
      photoUrl = data.photoUrl || '';
    } catch (e: any) {
      errorMsg = e.message || 'Failed to load profile';
    } finally {
      loading = false;
    }
  });

  async function handleSaveGeneral(e: Event) {
    e.preventDefault();
    saving = true;
    errorMsg = '';
    successMsg = '';
    
    try {
      const data = await api.put<any>('/profile', {
        firstName,
        lastName,
        phone,
        location,
        rowVersion: profile.rowVersion
      });
      profile = data;
      successMsg = 'Profile updated successfully!';
      setTimeout(() => successMsg = '', 3000);
    } catch (e: any) {
      errorMsg = e.message || 'Failed to update profile';
    } finally {
      saving = false;
    }
  }
</script>

<svelte:head>
  <title>My Profile - CV Nexus</title>
</svelte:head>

<div class="max-w-5xl mx-auto">
  <div class="flex items-center justify-between mb-8">
    <div>
      <h1 class="text-3xl font-bold text-slate-100">My Profile</h1>
      <p class="text-slate-400 mt-1">Manage your details, attributes, and projects.</p>
    </div>
  </div>

  {#if loading}
    <div class="flex items-center justify-center py-20">
      <Loader2 class="animate-spin text-indigo-500" size={40} />
    </div>
  {:else if profile}
    <!-- Tabs -->
    <div class="flex space-x-1 bg-slate-900/50 p-1 rounded-xl border border-slate-700/50 mb-8 max-w-md">
      <button 
        onclick={() => activeTab = 'general'}
        class="flex-1 flex items-center justify-center gap-2 py-2.5 px-4 rounded-lg text-sm font-medium transition-all {activeTab === 'general' ? 'bg-slate-800 text-white shadow-sm' : 'text-slate-400 hover:text-slate-200'}"
      >
        <User size={16} /> General
      </button>
      <button 
        onclick={() => activeTab = 'attributes'}
        class="flex-1 flex items-center justify-center gap-2 py-2.5 px-4 rounded-lg text-sm font-medium transition-all {activeTab === 'attributes' ? 'bg-slate-800 text-white shadow-sm' : 'text-slate-400 hover:text-slate-200'}"
      >
        <Settings size={16} /> Attributes
      </button>
      <button 
        onclick={() => activeTab = 'projects'}
        class="flex-1 flex items-center justify-center gap-2 py-2.5 px-4 rounded-lg text-sm font-medium transition-all {activeTab === 'projects' ? 'bg-slate-800 text-white shadow-sm' : 'text-slate-400 hover:text-slate-200'}"
      >
        <Briefcase size={16} /> Projects
      </button>
    </div>

    {#if errorMsg}
      <div class="mb-6 p-4 rounded-lg bg-red-500/10 border border-red-500/20 text-red-400 text-sm">
        {errorMsg}
      </div>
    {/if}
    {#if successMsg}
      <div class="mb-6 p-4 rounded-lg bg-green-500/10 border border-green-500/20 text-green-400 text-sm">
        {successMsg}
      </div>
    {/if}

    <div class="bg-slate-900/40 backdrop-blur-sm border border-slate-700/50 rounded-2xl p-6 md:p-8 shadow-xl">
      {#if activeTab === 'general'}
        <form onsubmit={handleSaveGeneral} class="max-w-2xl">
          <div class="flex items-center gap-6 mb-8">
            <div class="h-24 w-24 rounded-full bg-slate-800 border-2 border-slate-700 overflow-hidden flex items-center justify-center relative group cursor-pointer">
              {#if photoUrl}
                <img src={photoUrl} alt="Profile" class="h-full w-full object-cover" />
              {:else}
                <User size={40} class="text-slate-500" />
              {/if}
              <div class="absolute inset-0 bg-black/60 flex items-center justify-center opacity-0 group-hover:opacity-100 transition-opacity">
                <span class="text-xs font-semibold text-white">Upload</span>
              </div>
            </div>
            <div>
              <h3 class="text-lg font-medium text-slate-200">Profile Photo</h3>
              <p class="text-sm text-slate-400">JPG, PNG up to 5MB</p>
            </div>
          </div>

          <div class="grid grid-cols-1 md:grid-cols-2 gap-6 mb-6">
            <div>
              <label for="firstName" class="block text-sm font-medium text-slate-300 mb-1.5">First Name</label>
              <input 
                id="firstName" type="text" bind:value={firstName} required 
                class="w-full px-4 py-2.5 rounded-lg bg-slate-950 border border-slate-700 text-white focus:border-indigo-500 focus:ring-1 focus:ring-indigo-500 transition-colors"
              />
            </div>
            <div>
              <label for="lastName" class="block text-sm font-medium text-slate-300 mb-1.5">Last Name</label>
              <input 
                id="lastName" type="text" bind:value={lastName} required 
                class="w-full px-4 py-2.5 rounded-lg bg-slate-950 border border-slate-700 text-white focus:border-indigo-500 focus:ring-1 focus:ring-indigo-500 transition-colors"
              />
            </div>
          </div>

          <div class="grid grid-cols-1 md:grid-cols-2 gap-6 mb-8">
            <div>
              <label for="phone" class="block text-sm font-medium text-slate-300 mb-1.5">Phone (Optional)</label>
              <input 
                id="phone" type="text" bind:value={phone}
                class="w-full px-4 py-2.5 rounded-lg bg-slate-950 border border-slate-700 text-white focus:border-indigo-500 focus:ring-1 focus:ring-indigo-500 transition-colors"
              />
            </div>
            <div>
              <label for="location" class="block text-sm font-medium text-slate-300 mb-1.5">Location (Optional)</label>
              <input 
                id="location" type="text" bind:value={location}
                class="w-full px-4 py-2.5 rounded-lg bg-slate-950 border border-slate-700 text-white focus:border-indigo-500 focus:ring-1 focus:ring-indigo-500 transition-colors"
              />
            </div>
          </div>

          <button 
            type="submit" disabled={saving}
            class="px-6 py-2.5 rounded-lg bg-indigo-600 text-white font-medium hover:bg-indigo-500 transition-colors disabled:opacity-70 flex items-center"
          >
            {#if saving}
              <Loader2 class="animate-spin mr-2" size={18} />
              Saving...
            {:else}
              Save Changes
            {/if}
          </button>
        </form>
      {:else if activeTab === 'attributes'}
        <div class="text-center py-12">
          <Settings class="mx-auto h-12 w-12 text-slate-600 mb-3" />
          <h3 class="text-lg font-medium text-slate-300">Dynamic Attributes</h3>
          <p class="text-slate-500 mt-1 mb-6">Manage your specialized skills, education, and experience values.</p>
          <button class="px-4 py-2 rounded-lg bg-slate-800 border border-slate-700 text-slate-300 hover:bg-slate-700 transition-colors inline-flex items-center gap-2">
            <Plus size={16} /> Add Attribute
          </button>
        </div>
      {:else if activeTab === 'projects'}
        <div class="text-center py-12">
          <Briefcase class="mx-auto h-12 w-12 text-slate-600 mb-3" />
          <h3 class="text-lg font-medium text-slate-300">Projects & Experience</h3>
          <p class="text-slate-500 mt-1 mb-6">Highlight specific projects or roles you've held.</p>
          <button class="px-4 py-2 rounded-lg bg-slate-800 border border-slate-700 text-slate-300 hover:bg-slate-700 transition-colors inline-flex items-center gap-2">
            <Plus size={16} /> Add Project
          </button>
        </div>
      {/if}
    </div>
  {/if}
</div>
