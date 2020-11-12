import Agent
import random as rand

class LeagueUtil:
	def __init__(self, board, league):
		self.player_names = []
		self.board_agents = []
		self.league_agents = []

		self.player_names.append('learning strategy and tactics')
		self.board_agents.append(Agent.Agent(board, self.select_difficulty(), 'max'))
		self.league_agents.append(Agent.Agent(league, 'Assets\Scripts\TicTacToe\league.txt', 'max'))

		self.player_names.append('learning tactics only')
		self.board_agents.append(Agent.Agent(board, self.select_difficulty(), 'max'))
		self.league_agents.append(Agent.Agent(league, 'Assets\Scripts\TicTacToe\league.txt', 'random'))

		self.player_names.append('learning strategy only')
		self.board_agents.append(Agent.Agent(board, self.select_difficulty(), 'random'))
		self.league_agents.append(Agent.Agent(league, 'Assets\Scripts\TicTacToe\league.txt', 'max'))

		self.player_names.append('no learning')
		self.board_agents.append(Agent.Agent(board, self.select_difficulty(), 'random'))
		self.league_agents.append(Agent.Agent(league, 'Assets\Scripts\TicTacToe\league.txt', 'random'))

	def get_names(self):
		return self.player_names
		
	def get_boards(self):
		return self.board_agents
		
	def get_leagues(self):
		return self.league_agents
		
	def select_difficulty(self):
		diffdict = {1 : r'Assets\Scripts\TicTacToe\easy.txt',
                2 : r'Assets\Scripts\TicTacToe\medium.txt',
                3 : r'Assets\Scripts\TicTacToe\hard.txt'}
		return diffdict[rand.randint(1,3)]
		
	def get_board_agent(self, index):
		return self.board_agents[index]
		
	def get_league_agent(self, index):
		return self.league_agents[index]