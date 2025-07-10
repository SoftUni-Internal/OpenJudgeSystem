const initAnalytics = () => {
    const GA_ID = import.meta.env.VITE_GA_ID;
    if (!GA_ID || import.meta.env.DEV) {return;}

    const script = document.createElement('script');
    script.setAttribute('async', '');
    script.src = `https://www.googletagmanager.com/gtag/js?id=${GA_ID}`;
    document.head.appendChild(script);

    const inlineScript = document.createElement('script');
    inlineScript.innerHTML = `
    window.dataLayer = window.dataLayer || [];
    function gtag(){dataLayer.push(arguments);}
    gtag('js', new Date());
    gtag('config', '${GA_ID}');
  `;
    document.head.appendChild(inlineScript);
};

export default initAnalytics;
