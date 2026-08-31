<script lang="ts">
	import '../app.css';
	import favicon from '$lib/assets/favicon.svg';
	import TopNavbar from '$lib/components/common/TopNavbar.svelte';
	import Sidebar from '$lib/components/common/Sidebar.svelte';

	let { children } = $props();
	
	let sidebarOpen = $state(false);
	
	function toggleSidebar() {
	  sidebarOpen = !sidebarOpen;
	}
</script>

<svelte:head>
  <link rel="icon" href={favicon} />
</svelte:head>

<div class="min-h-screen bg-slate-950 text-slate-200 flex overflow-hidden">
  <Sidebar isOpen={sidebarOpen} closeSidebar={() => sidebarOpen = false} />
  
  <div class="flex flex-col flex-1 w-full min-w-0">
    <TopNavbar {toggleSidebar} />
    
    <main class="flex-1 overflow-auto p-4 md:p-8 relative">
      <!-- Ambient background glow for rich aesthetics -->
      <div class="absolute top-0 right-0 w-96 h-96 bg-indigo-500/10 rounded-full blur-3xl -z-10 pointer-events-none"></div>
      <div class="absolute bottom-0 left-0 w-96 h-96 bg-purple-500/10 rounded-full blur-3xl -z-10 pointer-events-none"></div>
      
      <div class="mx-auto max-w-7xl h-full">
        {@render children()}
      </div>
    </main>
  </div>
</div>
