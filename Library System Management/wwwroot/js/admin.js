document.addEventListener('DOMContentLoaded', function(){
  function updateTypeVisibility(root){
    var sel = root.querySelector('select[name="Input.Type"]');
    if(!sel) return;
    var val = sel.value;
    root.querySelectorAll('.type-specific').forEach(function(el){
      if(el.dataset.type === val) el.classList.add('active'); else el.classList.remove('active');
    });
  }

  // init for all admin pages
  document.querySelectorAll('.admin-card').forEach(function(card){
    updateTypeVisibility(card);
    var sel = card.querySelector('select[name="Input.Type"]');
    if(sel) sel.addEventListener('change', function(){ updateTypeVisibility(card); });
  });
});
