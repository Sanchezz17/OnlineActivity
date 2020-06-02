import React, { Component } from 'react';
import authorizeFetch from '../../utils/authorizeFetch';
import styles from './leaderboard.module.css';
import { UserStatistics } from './LeaderboardConstants';

const DEFAULT_LIMIT = 5;

export default class Leaderboard extends Component {
    constructor(props) {
        super(props);
        this.state = {
            bestUsers: [],
            currentUser: null,
            currentUserInBestUsers: false,
            currentUserPosition: -1,
            loadingUsers: true,
            desiredStatistics: UserStatistics.TOTAL_SCORE
        };
    }

    async componentDidMount() {
        await this.fetchCurrentUser();
        await this.fetchCurrentUserPosition();
        this.fetchUsers();
    }

    onChangeSelectedSort = (event) => {
        const { value } = event.target;
        this.setState({
            desiredStatistics: value
        });
        this.fetchCurrentUserPosition();
        this.fetchUsers();
    };

    fetchUsers = async (limit = DEFAULT_LIMIT) => {
        const bestUsers = await authorizeFetch(`/api/users?desiredStatistics=${this.state.desiredStatistics}&limit=${limit}`);
        this.setState({
            loadingUsers: false,
            bestUsers,
            currentUserInBestUsers: bestUsers.some(u => u.userName === this.state.currentUser.userName)
        });
    };

    fetchCurrentUser = async () => {
        const { user } = this.props;
        const currentUser = await authorizeFetch(`/api/users/${user.name}`);
        this.setState({
            currentUser
        });
    };
    
    fetchCurrentUserPosition = async () => {
        const { user } = this.props;
        const currentUserPosition = await authorizeFetch(`/api/users/${user.name}/position?desiredStatistics=${this.state.desiredStatistics}`);
        this.setState({
            currentUserPosition
        });
    };

    renderUser = (user, position) => {
        const { currentUserInBestUsers, currentUser } = this.state;
        return (
            <div className={styles.user} key={position}>
                <div className={styles.user__info}>
                    <span className={styles.user__index}>{position}</span>
                    {user.photoUrl &&
                        <img className={styles.user__image} src={user.photoUrl} alt={user.userName}/>
                    }
                    <span 
                        className={currentUserInBestUsers && user.userName === currentUser.userName 
                            ? `${styles.current__user}`
                            : `${styles.user__name}`}
                    >
                        {user.userName}
                    </span>
                </div>
                <p className={styles.user__score}>{user.statistics[this.state.desiredStatistics]}</p>
            </div>
        );
    };

    renderUsers = (users) => {
        return (
            <>
                {
                    this.state.currentUserInBestUsers
                        ? users.map((user, index) => this.renderUser(user, index + 1))
                        : <section>
                            {users.map((user, index) => this.renderUser(user, index + 1))}
                            <p className={styles.ellipsis}>...</p>
                            {this.renderUser(this.state.currentUser, this.state.currentUserPosition)}
                        </section>
                }
            </>
        );
    };

    getTableHeader = () => {
        switch (this.state.desiredStatistics) {
            case UserStatistics.TOTAL_SCORE:
                return 'Количество очков';
            case UserStatistics.NUMBER_OF_GAMES_PLAYED:
                return 'Количество сыгранных игр';
            case UserStatistics.NUMBER_OF_DRAWS:
                return 'Количество рисований';
            case UserStatistics.WINS_COUNT:
                return 'Количество побед';
        }
    };

    render() {
        const { bestUsers, loadingUsers } = this.state;
        return (
            <>
                <h1>Список лидеров</h1>

                {loadingUsers ? <p>Загрузка игроков...</p> :
                    <>
                        <section className={styles.select__sort}>
                            <label htmlFor="leaders" className={styles.select__label}>Сортировать</label>

                            <select name="leaders" id="leaders" onChange={this.onChangeSelectedSort}>
                                <option value={UserStatistics.TOTAL_SCORE} selected='selected'>по количеству очков
                                </option>
                                <option value={UserStatistics.NUMBER_OF_GAMES_PLAYED}>по количеству сыгранных игр
                                </option>
                                <option value={UserStatistics.NUMBER_OF_DRAWS}>по количеству рисований</option>
                                <option value={UserStatistics.WINS_COUNT}>по количеству побед</option>
                            </select>
                        </section>

                        <section className={styles.board}>
                            <section className={styles.table__header}>
                                <p className={styles.p__info}>Игрок</p>
                                <p className={styles.p__score}>{this.getTableHeader()}</p>
                            </section>
                            {this.renderUsers(bestUsers)}
                        </section>
                    </>
                }
            </>
        );
    }
}