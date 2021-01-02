if (document.querySelector('[data-component="PurchasePrice"] [data-component="Message"]').innerText === "Free" && 
    document.querySelector('[data-testid="purchase-cta-button"][data-component="BaseButton"]').innerText === "GET") {
    true;
} else {
    false;
}


