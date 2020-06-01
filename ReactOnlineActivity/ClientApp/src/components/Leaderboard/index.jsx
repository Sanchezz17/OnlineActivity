import React, { Component } from 'react';
import authorizeFetch from '../../utils/authorizeFetch';
import styles from './leaderboard.module.css';
import { UserStatistics } from './LeaderboardConstants';

const DEFAULT_LIMIT = 1;

export default class Leaderboard extends Component {
    constructor(props) {
        super(props);
        this.state = {
            bestUsers: [],
            loadingUsers: true,
            desiredStatistics: UserStatistics.TOTAL_SCORE,
            currentUserIndex: -1
        };
    }

    componentDidMount() {
        const { user } = this.props;
        this.getIndexOfCurrentUser(user);        
        this.fetchUsers(UserStatistics.TOTAL_SCORE);

    }

    onChangeSelectedSort = (event) => {
        const { value } = event.target;
        this.fetchUsers(value);
    };

    fetchUsers = async (desiredStatistics, limit = DEFAULT_LIMIT) => {
        const bestUsers = await authorizeFetch(`/api/users?desiredStatistics=${desiredStatistics}&limit=${limit}`);
        this.setState({
            loadingUsers: false,
            bestUsers,
            desiredStatistics
        });
    };
    
    getIndexOfCurrentUser = async (currentUser) => {
        const allUsers = await authorizeFetch(`/api/users?desiredStatistics=${this.state.desiredStatistics}&limit=${-1}`);
        this.setState({
            currentUserIndex: allUsers.findIndex(u => u.userName === currentUser.name)
        });
    }

    renderUsers(users, userInBestUsers, currentUser) {
        let index = 0;
        const renderUser = user => (
            <div className={styles.user} key={user.userName}>
                <div className={styles.user__info}>
                    <span className={styles.user__index}>{++index}</span>
                    {user.photoUrl &&
                    <img className={styles.user__image} src={user.photoUrl} alt={user.userName}/>}
                    <span className={userInBestUsers ? user.userName === userInBestUsers.userName ?
                        `${styles.current__user}`:`${styles.user__name}`: `${styles.user__name}`}>{user.userName}</span>
                </div>
                <p className={styles.user__score}>{user.statistics[this.state.desiredStatistics]}</p>
            </div>
        );
        
        return (<>
            {
                userInBestUsers ? users.map(user => renderUser(user)): 
                    <section>
                        {users.map(user => renderUser(user))}
                        <p className={styles.ellipsis}>...</p>
                        <div className={styles.user} key={currentUser.name}>
                            <div className={styles.user__info}>
                                <span className={styles.user__index}>{this.state.currentUserIndex + 1}</span>
                                {currentUser.photoUrl &&
                                <img className={styles.user__image} src={currentUser.photoUrl} alt={currentUser.name}/>}
                                <span className={`${styles.current__user}`}>{currentUser.name}</span>
                            </div>
                            <p className={styles.user__score}>{JSON.parse(currentUser.statistics)[this.state.desiredStatistics]}</p>
                        </div>
                    </section>
            }
        </>);
    }
    
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
    }

    render() {
        const { bestUsers, loadingUsers } = this.state;
        const { user } = this.props;
        const currentUser = user;
        const userInBestUsers = bestUsers.find(u => u.userName === user.preferred_username);
        return (
            <>
                <h1>Список лидеров</h1>

                {loadingUsers ? <p>Загрузка игроков...</p> :
                    <>
                        <section className={styles.select__sort}>
                            <label htmlFor="leaders" className={styles.select__label}>Сортировать</label>

                            <select name="leaders" id="leaders" onChange={this.onChangeSelectedSort}>
                                <option value={UserStatistics.TOTAL_SCORE} selected='selected'>по количеству очков</option>
                                <option value={UserStatistics.NUMBER_OF_GAMES_PLAYED}>по количеству сыгранных игр</option>
                                <option value={UserStatistics.NUMBER_OF_DRAWS}>по количеству рисований</option>
                                <option value={UserStatistics.WINS_COUNT}>по количеству побед</option>
                            </select>
                        </section>

                        <section className={styles.board}>
                            <section className={styles.table__header}>
                                <p className={styles.p__info}>Игрок</p>
                                <p className={styles.p__score}>{this.getTableHeader()}</p>
                            </section>
                            {this.renderUsers(bestUsers, userInBestUsers, currentUser)}
                        </section>
                    </>
                }
            </>
        );
    }
}