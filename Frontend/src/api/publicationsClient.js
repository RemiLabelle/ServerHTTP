import apiClient from './apiClient'

const gamesClient = {
  all: (params = {}) => apiClient.get('/publications', { params }),
  create: (title, content) => apiClient.post('/publications', { title, content }),
}

export default gamesClient
