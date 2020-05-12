import React, {Component} from 'react';
import {Stage, Layer, Line} from 'react-konva';
import styles from './field.module.css';

export default class Field extends Component {
    constructor(props) {
        super(props);
        this.state = {
            lines: [],
            loadingField: true,
            drawing: false,
            stageRef: undefined,
            isActiveUser: true,
            activeUser: undefined,
            users: []
        };
    }

    componentDidMount() {
        setInterval(this.fetchLines, 500);
    }

    getActiveClient = () => {
        fetch(`/api/roomActive/${this.props.roomId}`)
            .then(response => response.json())
            .then(activeClient => this.setState({
                ...this.state,
                activeUser: activeClient.user
            }, () => {
                if (!this.state.activeUser) {
                    this.fetchUsers();
                } else {
                    if (this.state.activeUser.username === this.props.user.username) {
                        this.setState({
                            ...this.state,
                            isActiveUser: true
                        })
                    }
                }
            }));
    }

    setActiveClient = () => {
        fetch(`/api/roomActive/${this.props.roomId}`, {
            body: JSON.stringify({activeClient: this.state.activeUser}),
            headers: {'Content-Type': 'application/json'},
            method: 'POST'
        }).then(this.getActiveClient);
    }

    fetchUsers = () => {
        const setStateCallback = () => {
            const randomUser = this.state.users[Math.floor(Math.random() * this.state.users.length)]
            if (randomUser.username === this.props.user.username) {
                this.setState({
                    ...this.state,
                    isActiveUser: true,
                    activeUser: randomUser
                }, this.setActiveClient)
            }
        }

        fetch(`/api/users/${this.props.roomId}`)
            .then(response => response.json())
            .then(users => this.setState({
                ...this.state,
                users
            }, setStateCallback))
    };

    fetchLines = () => {
        fetch(`/api/field/${this.props.roomId}`)
            .then(response => response.json())
            .then(lines => this.setState({
                ...this.state,
                loadingField: false,
                lines
            }))
    }

    addLines = () => {
        fetch(`/api/field/${this.props.roomId}`, {
            body: JSON.stringify(this.state.lines),
            headers: {'Content-Type': 'application/json'},
            method: 'POST'
        })
    };

    handleMouseDown = () => {
        this.state.drawing = true;
        this.setState({
            lines: [...this.state.lines, []]
        });
        this.addLines();
    };

    handleMouseUp = () => {
        this.state.drawing = false;
    };

    handleMouseMove = e => {
        if (!this.state.drawing) {
            return;
        }
        const stage = this.state.stageRef.getStage();
        const point = stage.getPointerPosition();
        const {lines} = this.state;
        let lastLine = lines[lines.length - 1];
        lastLine = lastLine.concat([point.x, point.y]);
        lines.splice(lines.length - 1, 1, lastLine);
        this.setState({
            lines: lines.concat()
        });
        this.addLines();
    };

    clearField = () => {
        this.setState({...this.state, lines: []}, this.addLines);
    }

    render() {
        if (typeof window === 'undefined') {
            return null;
        }

        return (
            <section className={styles.field}>
                {this.state.isActiveUser ?
                    <section className={'drawerTools'}>
                        <button
                            className={`btn btn-outline-warning btn-sm ${styles.clear}`}
                            onClick={this.clearField}
                        >
                            Очистить
                        </button>
                        <button
                            className={`btn btn-outline-primary btn-sm ${styles.end}`}
                        >
                            Завершить
                        </button>
                    </section> : ''
                }
                {this.state.loadingField
                    ? <div className={styles.loading}>
                        <p>Загрузка...</p>
                    </div>
                    : <Stage
                        width={window.innerWidth}
                        height={window.innerHeight}
                        onContentMousedown={this.state.isActiveUser ? this.handleMouseDown : () => {}}
                        onContentMousemove={this.state.isActiveUser ? this.handleMouseMove : () => {}}
                        onContentMouseup={this.state.isActiveUser ? this.handleMouseUp : () => {}}
                        ref={node => {
                            this.state.stageRef = node;
                        }}>
                        <Layer>
                            {this.state.lines.map((line, i) => (
                                <Line key={i} points={line} stroke="blue"/>
                            ))}
                        </Layer>
                    </Stage>
                }
            </section>
        )
    }
}
