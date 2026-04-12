/**
 * API Client for Book of Receipts
 */
const api = {
  baseUrl: "/api",

  /**
   * Generic fetch wrapper with error handling
   */
  async request(endpoint, options = {}) {
    const url = `${this.baseUrl}${endpoint}`;
    const config = {
      headers: {
        "Content-Type": "application/json",
        ...options.headers,
      },
      ...options,
    };

    try {
      const response = await fetch(url, config);

      if (!response.ok) {
        const errorText = await response.text();
        let errorMessage = `HTTP ${response.status}`;

        try {
          const errorData = JSON.parse(errorText);
          if (errorData.errors) {
            errorMessage = Object.values(errorData.errors).flat().join(", ");
          } else if (errorData.message) {
            errorMessage = errorData.message;
          } else if (errorData.error) {
            errorMessage = errorData.error;
          }
        } catch {
          // Keep default error message
        }

        throw new Error(errorMessage);
      }

      // Handle 204 No Content
      if (response.status === 204) {
        return null;
      }

      return await response.json();
    } catch (error) {
      console.error("API Error:", error);
      throw error;
    }
  },

  // Products API
  products: {
    getAll(params = {}) {
      const queryString = new URLSearchParams();
      if (params.search) queryString.append("Search", params.search);
      if (params.category) queryString.append("Category", params.category);
      if (params.cookingRequirement)
        queryString.append("CookingRequirement", params.cookingRequirement);
      if (params.flags)
        params.flags.forEach((f) => queryString.append("Flags", f));
      if (params.sort) queryString.append("Sort", params.sort);
      if (params.ascending !== undefined)
        queryString.append("Ascending", params.ascending);

      return api.request(`/Products?${queryString.toString()}`);
    },

    getById(id) {
      return api.request(`/Products/${id}`);
    },

    create(data) {
      return api.request("/Products", {
        method: "POST",
        body: JSON.stringify(data),
      });
    },

    update(id, data) {
      return api.request(`/Products/${id}`, {
        method: "PATCH",
        body: JSON.stringify(data),
      });
    },

    delete(id) {
      return api.request(`/Products/${id}`, {
        method: "DELETE",
      });
    },
  },

  // Dishes API
  dishes: {
    getAll(params = {}) {
      const queryString = new URLSearchParams();
      if (params.search) queryString.append("Search", params.search);
      if (params.category) queryString.append("Category", params.category);
      if (params.flags)
        params.flags.forEach((f) => queryString.append("Flags", f));
      if (params.sort) queryString.append("Sort", params.sort);
      if (params.ascending !== undefined)
        queryString.append("Ascending", params.ascending);

      return api.request(`/Dishes?${queryString.toString()}`);
    },

    getById(id) {
      return api.request(`/Dishes/${id}`);
    },

    create(data) {
      return api.request("/Dishes", {
        method: "POST",
        body: JSON.stringify(data),
      });
    },

    update(id, data) {
      return api.request(`/Dishes/${id}`, {
        method: "PATCH",
        body: JSON.stringify(data),
      });
    },

    delete(id) {
      return api.request(`/Dishes/${id}`, {
        method: "DELETE",
      });
    },
  },
};
