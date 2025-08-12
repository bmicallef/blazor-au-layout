let childWindows = new Map();
let channel = null;
let layoutId = null;

export function initChannel(id){
  layoutId = id;
  channel = new BroadcastChannel(`gl-sync-${id}`);
  return true;
}

export function post(msg){
  if(!channel) return;
  channel.postMessage(msg);
}

export function onMessage(dotnetRef){
  if(!channel) return;
  channel.onmessage = (ev)=>{
    dotnetRef.invokeMethodAsync('OnChannelMessage', JSON.stringify(ev.data));
  };
}

export function openPopout(url, name, features){
  const w = window.open(url, name || "gl-popout", features || "width=800,height=600");
  if(!w) return null;
  const id = crypto.randomUUID();
  childWindows.set(id, w);
  const timer = setInterval(()=>{
    if(w.closed){
      clearInterval(timer);
      childWindows.delete(id);
      window.dispatchEvent(new CustomEvent('gl-popout-closed', { detail:{ id } }));
      post({ t:'popoutClosed', id, layoutId });
    }
  }, 400);
  setTimeout(()=>post({ t:'helloChild', id, layoutId }), 50);
  return id;
}

export function closePopout(id){
  const w = childWindows.get(id);
  if(w && !w.closed) w.close();
  childWindows.delete(id);
}

export function onClosed(dotnetRef){
  window.addEventListener('gl-popout-closed', e=>{
    dotnetRef.invokeMethodAsync('OnPopoutClosed', e.detail.id);
  });
}
