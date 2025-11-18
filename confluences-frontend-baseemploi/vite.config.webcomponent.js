import { defineConfig } from "vite";
import vue from "@vitejs/plugin-vue";
import path from "path";

export default defineConfig({
  plugins: [vue()],
  build: {
    lib: {
      entry: path.resolve(__dirname, "src/webcomponents/login-webcomponent.js"),
      name: "ConfluencesLogin",
      fileName: () => `login-webcomponent.mjs`,
      formats: ["es"]
    },
    outDir: "../src/mvc/wwwroot/js",
    emptyOutDir: false
  }
});
