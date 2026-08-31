<script lang="ts">
  import { onMount } from 'svelte';
  import { authState } from '$lib/state/auth.svelte';
  import { goto } from '$app/navigation';
  import { Loader2 } from '@lucide/svelte';

  onMount(() => {
    // Check URL search params for token (e.g. ?token=xxxxx)
    const urlParams = new URLSearchParams(window.location.search);
    const token = urlParams.get('token');

    if (token) {
      authState.setToken(token);
      goto('/');
    } else {
      // Check hash fragment (e.g. #token=xxxxx) just in case
      const hashParams = new URLSearchParams(window.location.hash.substring(1));
      const hashToken = hashParams.get('token');
      
      if (hashToken) {
        authState.setToken(hashToken);
        goto('/');
      } else {
        // No token found, go to login with error
        goto('/login?error=AuthenticationFailed');
      }
    }
  });
</script>

<svelte:head>
  <title>Authenticating... - CV Nexus</title>
</svelte:head>

<div class="flex flex-col items-center justify-center min-h-[80vh]">
  <div class="w-16 h-16 rounded-full bg-slate-800/50 flex items-center justify-center mb-6 shadow-xl border border-slate-700">
    <Loader2 class="animate-spin text-indigo-400" size={32} />
  </div>
  <h2 class="text-xl font-bold text-slate-200">Completing Authentication</h2>
  <p class="text-slate-400 mt-2">Please wait while we log you in...</p>
</div>
