import Agent
import random as rand
import os

class LeagueUtil:
	def __init__(self, board, league):
		self.player_names = []
		self.board_agents = []
		self.league_agents = []

		#store all AI available for play during league play that will be randomly selected
		self.player_names.append('learning strategy and tactics')
		self.board_agents.append(Agent.Agent(board, self.select_difficulty(), 'max'))
		self.league_agents.append(Agent.Agent(league, os.getcwd() + r'\Resources\TicTacToe\league.txt', 'max'))

		self.player_names.append('learning tactics only')
		self.board_agents.append(Agent.Agent(board, self.select_difficulty(), 'max'))
		self.league_agents.append(Agent.Agent(league, os.getcwd() + r'\Resources\TicTacToe\league.txt', 'random'))

		self.player_names.append('learning strategy only')
		self.board_agents.append(Agent.Agent(board, self.select_difficulty(), 'random'))
		self.league_agents.append(Agent.Agent(league, os.getcwd() + r'\Resources\TicTacToe\league.txt', 'max'))

		self.player_names.append('no learning')
		self.board_agents.append(Agent.Agent(board, self.select_difficulty(), 'random'))
		self.league_agents.append(Agent.Agent(league, os.getcwd() + r'\Resources\TicTacToe\league.txt', 'random'))

	def get_names(self):
		return self.player_names
		
	def get_boards(self):
		return self.board_agents
		
	def get_leagues(self):
		return self.league_agents
		
	#randomly select the difficulty of the generated AI
	def select_difficulty(self):
		diffdict = {1 : os.getcwd() + r'\Resources\TicTacToe\easy.txt',
                2 : os.getcwd() + r'\Resources\TicTacToe\medium.txt',
                3 : os.getcwd() + r'\Resources\TicTacToe\hard.txt'}
		return diffdict[rand.randint(1,3)]
		
	def get_board_agent(self, index):
		return self.board_agents[index]
		
	def get_league_agent(self, index):
		return self.league_agents[index]
		
	def get_agent_name(self, index):
		return self.player_names[index]