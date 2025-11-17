(() => {
  // Atalhos para querySelector
  const $=(s,r=document)=>r.querySelector(s), $$=(s,r=document)=>[...r.querySelectorAll(s)];
  const fmt=n=>n.toLocaleString("pt-PT",{style:"currency",currency:"EUR"}); // formatar €€

  // MENU / HEADER
  const nav=$("#nav"), hamb=$("#hamburger");
  if(hamb) hamb.onclick=()=>{ 
    let aberto=nav.classList.toggle("open"); 
    hamb.setAttribute("aria-expanded",aberto) 
  };
  if($("#year")) $("#year").textContent=new Date().getFullYear(); // ano no footer

  // CARRINHO (localStorage)
  const getCart=()=>JSON.parse(localStorage.getItem("cart")||"[]");
  const setCart=c=>localStorage.setItem("cart",JSON.stringify(c));
  const badge=()=>{ let b=$("#cart-count"); if(b) b.textContent=getCart().reduce((s,i)=>s+i.qty,0); };
  badge(); // atualizar logo no load

  // Toast (mensagem rápida no fundo do ecrã)
  const toast=msg=>{
    let t=document.createElement("div");
    t.textContent=msg; t.className="toast"; 
    document.body.appendChild(t);
    setTimeout(()=>{t.style.opacity=0; setTimeout(()=>t.remove(),300)},1200);
  };

  // Função para adicionar item ao carrinho
  const add=(id,q=1)=>{
    let c=getCart(),i=c.findIndex(x=>x.id===id);
    i>=0?c[i].qty+=q:c.push({id,qty:q});
    setCart(c); badge(); toast("Adicionado ao carrinho");
  };

  // CRIAÇÃO DE CARDS DE PRODUTO
  const card=p=>`
  <article class="card">
    <img src="${p.img||'../img/placeholder.png'}" alt="${p.nome}">
    <div class="body">
      <h3>${p.nome}</h3><p class="muted">${p.desc}</p>
      <div class="row">
        <span class="price">${fmt(p.preco)}</span>
        <div>
          <a class="btn btn-ghost" href="product.html?id=${p.id}">Ver</a>
          <button class="btn btn-primary" data-add="${p.id}">Adicionar</button>
        </div>
      </div>
    </div>
  </article>`;

  // INDEX (Destaques)
  if($("#slider")) $("#slider").innerHTML=PRODUCTS.slice(0,6).map(card).join("");

  // LISTA DE PRODUTOS + FILTROS
  const grid=$("#product-grid");
  if(grid){
    const s=$("#search"),c=$("#category"),o=$("#sort");
    const render=()=>{
      let q=(s.value||"").toLowerCase(),cat=c.value;
      let list=PRODUCTS.filter(p=>(!cat||p.categoria===cat)&&(p.nome+p.desc).toLowerCase().includes(q));
      if(o.value==="preco-asc") list.sort((a,b)=>a.preco-b.preco);
      if(o.value==="preco-desc") list.sort((a,b)=>b.preco-a.preco);
      grid.innerHTML=list.map(card).join("");
    };
    [s,c,o].forEach(el=>el.oninput=render); render();
  }

  // PÁGINA DE PRODUTO INDIVIDUAL
  const pid=+new URLSearchParams(location.search).get("id");
  if(pid){let p=PRODUCTS.find(x=>x.id===pid);
    if(p){
      $("#prod-name").textContent=p.nome;
      $("#prod-price").textContent=fmt(p.preco);
      $("#prod-desc").textContent=p.desc;
      $("#prod-image").innerHTML=`<img src="${p.img}" alt="${p.nome}" class="bigimg">`;
      $("#add-to-cart").onclick=()=>add(p.id,Math.max(1,+$("#qty").value||1));
    }
  }

  // FORMULÁRIOS (Newsletter + Contacto)
  const validEmail=e=>/^[^\s@]+@[^\s@]+\.[^\s@]+$/.test(e);

  // Newsletter
  if($("#newsletter-form")) $("#newsletter-form").onsubmit=e=>{
    e.preventDefault();
    let em=$("#news-email").value.trim(),m=$("#news-msg");
    if(!validEmail(em)) m.textContent="Email inválido",m.style.color="#f55";
    else m.textContent="Subscrição concluída!",m.style.color="var(--accent)",e.target.reset();
  };

  // Contacto
  if($("#contact-form")) $("#contact-form").onsubmit=e=>{
    e.preventDefault(); let ok=1;
    $$(".form-field",e.target).forEach(f=>{
      let i=$("input,textarea",f),msg=$(".form-msg",f);
      if(!i.value.trim()) ok=0,msg.textContent="Obrigatório";
      else if(i.type==="email"&&!validEmail(i.value)) ok=0,msg.textContent="Email inválido";
      else msg.textContent="";
    });
    let r=$("#contact-result"); 
    r.textContent=ok?"Mensagem enviada (simulado).":"Verifica os campos."; 
    r.style.color=ok?"var(--accent)":"var(--danger)"; 
    if(ok) e.target.reset();
  };

  // CARRINHO
  const items=$("#cart-items");
  if(items){
    const render=()=>{
      let c=getCart(); items.innerHTML="";
      if(!c.length){$("#cart-empty").style.display="block"; $("#cart-summary").style.display="none"; return;}
      $("#cart-empty").style.display="none"; $("#cart-summary").style.display="block";
      let sub=0;
      c.forEach(r=>{
        let p=PRODUCTS.find(x=>x.id===r.id),t=p.preco*r.qty; sub+=t;
        items.innerHTML+=`
        <div class="cart-item">
          <img src="${p.img||'../img/placeholder.png'}">
          <div>${p.nome}<div class="muted">${fmt(p.preco)}</div></div>
          <div class="qty">
            <button data-dec="${p.id}">−</button>
            <span>${r.qty}</span>
            <button data-inc="${p.id}">+</button>
          </div>
          <div>${fmt(t)}</div>
          <button data-rm="${p.id}">✕</button>
        </div>`;
      });
      $("#subtotal").textContent=$("#total").textContent=fmt(sub);
    };
    render();

    // botões dentro do carrinho
    items.onclick=e=>{
      let id=+e.target.dataset.inc||+e.target.dataset.dec||+e.target.dataset.rm; 
      if(!id)return;
      let c=getCart(),i=c.findIndex(x=>x.id===id); if(i<0)return;
      if(e.target.dataset.inc) c[i].qty++;
      if(e.target.dataset.dec) c[i].qty=Math.max(1,c[i].qty-1);
      if(e.target.dataset.rm) c.splice(i,1);
      setCart(c); render(); badge();
    };

    // Checkout simulado
    $("#checkout").onclick=()=>toast("Checkout simulado");
  }

  // DELEGAÇÃO (para todos os botões de adicionar)
  document.body.onclick=e=>{ let id=e.target.dataset?.add; if(id) add(+id,1); };
})();
