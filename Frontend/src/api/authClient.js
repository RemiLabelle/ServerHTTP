import apiClient from './apiClient'

const authClient = {
  register: (username, password) => apiClient.post('/register', { username, password }),
  login: (username, password) => apiClient.post('/login', { username, password }),
}

export default authClient