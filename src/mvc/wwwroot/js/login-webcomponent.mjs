class i extends HTMLElement {
  constructor() {
    super(), this.attachShadow({ mode: "open" });
  }
  connectedCallback() {
    this.render(), this.shadowRoot.querySelector("#loginForm").addEventListener("submit", (e) => this.handleLogin(e));
  }
  async handleLogin(e) {
    e.preventDefault();
    const o = this.shadowRoot.querySelector("#email").value.trim(), n = this.shadowRoot.querySelector("#password").value.trim(), r = this.getAttribute("idp-url") ?? "http://localhost:5000", t = await fetch(`${r}/connect/token`, {
      method: "POST",
      headers: { "Content-Type": "application/x-www-form-urlencoded" },
      body: new URLSearchParams({
        grant_type: "password",
        client_id: "vue",
        client_secret: "secret",
        username: o,
        password: n,
        scope: "openid profile api1 roles offline_access"
      })
    });
    if (!t.ok) {
      alert("Identifiants incorrects");
      return;
    }
    const s = await t.json();
    this.dispatchEvent(
      new CustomEvent("confluences-auth-success", {
        detail: s,
        bubbles: !0,
        composed: !0
      })
    );
  }
  render() {
    this.shadowRoot.innerHTML = `
        <style>
            .container {
                max-width: 360px;
                margin: 80px auto;
                padding: 32px;
                background: white;
                border-radius: 16px;
                box-shadow: 0 4px 20px rgba(0,0,0,0.1);
                font-family: Arial, sans-serif;
            }
            h2 {
                margin: 0 0 16px 0;
                text-align: center;
                font-size: 24px;
                color: #333;
            }
            input {
                width: 100%;
                padding: 12px;
                margin: 8px 0;
                border-radius: 8px;
                border: 1px solid #ccc;
                font-size: 14px;
            }
            button {
                width: 100%;
                margin-top: 16px;
                padding: 12px;
                background: #3b82f6;
                border: none;
                border-radius: 8px;
                color: white;
                font-size: 16px;
                cursor: pointer;
            }
            button:hover {
                background: #2563eb;
            }
        </style>

        <div class="container">
            <h2>Connexion Confluences</h2>
            <form id="loginForm">
                <input id="email" type="email" placeholder="Email" required />
                <input id="password" type="password" placeholder="Mot de passe" required />
                <button type="submit">Se connecter</button>
            </form>
        </div>
        `;
  }
}
customElements.define("confluences-login", i);
